using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Student_Onboarding_Platform.Extensions;
using Student_Onboarding_Platform.Models.DTOs.Student;
using Student_Onboarding_Platform.Data.Repositories.Interfaces;
using Student_Onboarding_Platform.Models.DTOs.Common;
using Student_Onboarding_Platform.Models.DTOs.Course;
using Student_Onboarding_Platform.Models.Entities;
using Student_Onboarding_Platform.Services.Interfaces;

namespace Student_Onboarding_Platform.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Student")]
public class StudentController : ControllerBase
{
    private readonly IStudentService _studentService;
    private readonly INotificationService _notificationService;
    private readonly ICourseRepository _courseRepository;
    private readonly ICourseRegistrationRepository _registrationRepository;
    private readonly ICourseReviewRepository _reviewRepository;
    private readonly IUserService _userService;
    private readonly IFaqService _faqService;
    private readonly IInvoiceService _invoiceService;

    public StudentController(
        IStudentService studentService,
        INotificationService notificationService,
        ICourseRepository courseRepository,
        ICourseRegistrationRepository registrationRepository,
        ICourseReviewRepository reviewRepository,
        IUserService userService,
        IFaqService faqService,
        IInvoiceService invoiceService)
    {
        _studentService = studentService;
        _notificationService = notificationService;
        _courseRepository = courseRepository;
        _registrationRepository = registrationRepository;
        _reviewRepository = reviewRepository;
        _userService = userService;
        _faqService = faqService;
        _invoiceService = invoiceService;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.GetUserId();
        var result = await _studentService.GetProfileAsync(userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = User.GetUserId();
        var result = await _studentService.UpdateProfileAsync(userId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("profile/photo")]
    public async Task<IActionResult> UploadProfilePhoto(IFormFile photo)
    {
        if (photo == null || photo.Length == 0)
            return BadRequest(new { success = false, message = "No photo provided." });

        var userId = User.GetUserId();
        var result = await _studentService.UploadProfilePhotoAsync(userId, photo);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var userId = User.GetUserId();
        var result = await _studentService.GetDashboardAsync(userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("courses")]
    public async Task<IActionResult> GetRegisteredCourses()
    {
        var userId = User.GetUserId();
        var result = await _studentService.GetRegisteredCoursesAsync(userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("courses/register")]
    public async Task<IActionResult> RegisterForCourse([FromBody] CourseRegistrationRequest request)
    {
        var userId = User.GetUserId();
        var result = await _studentService.RegisterForCourseAsync(userId, request);

        if (result.Success)
        {
            var course = await _courseRepository.GetByIdAsync(request.CourseId);
            var courseName = course?.Name ?? "a course";

            // Notify the student
            await _notificationService.CreateStudentNotificationAsync(
                userId,
                "CourseRegistration",
                "Course Registration Submitted",
                $"You have registered for \"{courseName}\". Your registration is under review. Please wait for admin approval.",
                request.CourseId);

            // Notify all admins
            var student = await _userService.GetByIdAsync(userId);
            if (student != null)
            {
                await _notificationService.NotifyAdminsOfCourseRegistrationAsync(student, courseName, request.CourseId);
            }
        }

        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Student Notification Endpoints

    [HttpGet("notifications")]
    public async Task<IActionResult> GetNotifications()
    {
        var userId = User.GetUserId();
        var result = await _notificationService.GetStudentNotificationsAsync(userId);
        return Ok(result);
    }

    [HttpPut("notifications/{id}/read")]
    public async Task<IActionResult> MarkNotificationAsRead(Guid id)
    {
        var userId = User.GetUserId();
        var result = await _notificationService.MarkStudentNotificationAsReadAsync(id, userId);
        return Ok(result);
    }

    // Course Review Endpoints

    [HttpPost("courses/{courseId}/review")]
    public async Task<IActionResult> SubmitReview(Guid courseId, [FromBody] SubmitReviewRequest request)
    {
        var userId = User.GetUserId();

        if (request.Rating < 1 || request.Rating > 5)
            return BadRequest(ApiResponse<string>.Fail("Rating must be between 1 and 5."));

        // Check if student has completed this course
        var registration = await _registrationRepository.GetByUserAndCourseAsync(userId, courseId);
        if (registration == null)
            return BadRequest(ApiResponse<string>.Fail("You are not registered for this course."));
        if (!registration.IsCompleted)
            return BadRequest(ApiResponse<string>.Fail("You can only review a course after completing it."));

        var existing = await _reviewRepository.GetByUserAndCourseAsync(userId, courseId);
        if (existing != null)
            return BadRequest(ApiResponse<string>.Fail("You have already reviewed this course."));

        var review = new CourseReview
        {
            CourseId = courseId,
            UserId = userId,
            Rating = request.Rating,
            Remarks = request.Remarks
        };

        await _reviewRepository.CreateAsync(review);
        return Ok(ApiResponse<string>.Ok("Review submitted successfully."));
    }

    [AllowAnonymous]
    [HttpGet("courses/{courseId}/reviews")]
    public async Task<IActionResult> GetCourseReviews(Guid courseId)
    {
        var reviews = await _reviewRepository.GetByCourseIdAsync(courseId);
        var avgRating = await _reviewRepository.GetAverageRatingAsync(courseId);
        var count = await _reviewRepository.GetReviewCountAsync(courseId);

        var response = new List<CourseReviewResponse>();
        foreach (var r in reviews)
        {
            var user = await _userService.GetByIdAsync(r.UserId);
            response.Add(new CourseReviewResponse
            {
                Id = r.Id,
                CourseId = r.CourseId,
                UserId = r.UserId,
                StudentName = user != null ? $"{user.FirstName} {user.LastName}" : "Anonymous",
                Rating = r.Rating,
                Remarks = r.Remarks,
                CreatedAt = r.CreatedAt
            });
        }

        // Check if current user can review (completed + hasn't reviewed yet)
        var canReview = false;
        var hasReviewed = false;
        try
        {
            var userId = User.GetUserId();
            var reg = await _registrationRepository.GetByUserAndCourseAsync(userId, courseId);
            var existingReview = await _reviewRepository.GetByUserAndCourseAsync(userId, courseId);
            canReview = reg != null && reg.IsCompleted && existingReview == null;
            hasReviewed = existingReview != null;
        }
        catch { /* anonymous user, no claims */ }

        return Ok(ApiResponse<object>.Ok(new
        {
            AverageRating = avgRating.HasValue ? Math.Round(avgRating.Value, 1) : 0,
            TotalReviews = count,
            CanReview = canReview,
            HasReviewed = hasReviewed,
            Reviews = response
        }));
    }

    [HttpGet("faqs")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFaqs()
    {
        var result = await _faqService.GetActiveFaqsAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ── Invoices (read-only for students) ──────────────────────────────

    [HttpGet("courses/invoices")]
    public async Task<IActionResult> GetMyInvoices()
    {
        var userId = User.GetUserId();
        var result = await _invoiceService.GetForStudentAsync(userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("courses/invoices/{id}")]
    public async Task<IActionResult> GetMyInvoice(Guid id)
    {
        var userId = User.GetUserId();
        var result = await _invoiceService.GetByIdForStudentAsync(id, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("courses/registrations/{registrationId}/invoice")]
    public async Task<IActionResult> GetInvoiceForRegistration(Guid registrationId)
    {
        var userId = User.GetUserId();
        var result = await _invoiceService.GetOrCreateForRegistrationAsync(registrationId, userId, isAdmin: false);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
