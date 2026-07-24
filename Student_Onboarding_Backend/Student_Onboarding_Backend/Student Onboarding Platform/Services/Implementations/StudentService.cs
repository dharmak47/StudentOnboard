using Microsoft.AspNetCore.Http;
using Student_Onboarding_Platform.Data.Repositories.Interfaces;
using Student_Onboarding_Platform.Models.DTOs.Common;
using Student_Onboarding_Platform.Models.DTOs.Student;
using Student_Onboarding_Platform.Models.Entities;
using Student_Onboarding_Platform.Services.Interfaces;

namespace Student_Onboarding_Platform.Services.Implementations;

public class StudentService : IStudentService
{
    private readonly IUserService _userService;
    private readonly ICourseRepository _courseRepository;
    private readonly ICourseRegistrationRepository _registrationRepository;
    private readonly IEmailService _emailService;
    private readonly IFileStorageService _fileStorage;
    private readonly ILogger<StudentService> _logger;

    public StudentService(
        IUserService userService,
        ICourseRepository courseRepository,
        ICourseRegistrationRepository registrationRepository,
        IEmailService emailService,
        IFileStorageService fileStorage,
        ILogger<StudentService> logger)
    {
        _userService = userService;
        _courseRepository = courseRepository;
        _registrationRepository = registrationRepository;
        _emailService = emailService;
        _fileStorage = fileStorage;
        _logger = logger;
    }

    public async Task<ApiResponse<StudentProfileResponse>> GetProfileAsync(Guid userId)
    {
        var user = await _userService.GetByIdAsync(userId);
        if (user == null)
            return ApiResponse<StudentProfileResponse>.Fail("User not found.");

        return ApiResponse<StudentProfileResponse>.Ok(MapToProfile(user));
    }

    public async Task<ApiResponse<StudentProfileResponse>> UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
    {
        var user = await _userService.GetByIdAsync(userId);
        if (user == null)
            return ApiResponse<StudentProfileResponse>.Fail("User not found.");

        await _userService.UpdateProfileAsync(userId, request.FirstName, request.LastName, request.PhoneNumber,
            request.DateOfBirth, request.Address, request.Education);
        _logger.LogInformation("Profile updated for user {UserId}", userId);

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.PhoneNumber = request.PhoneNumber;
        user.DateOfBirth = request.DateOfBirth;
        user.Address = request.Address;
        user.Education = request.Education;

        return ApiResponse<StudentProfileResponse>.Ok(MapToProfile(user), "Profile updated successfully.");
    }

    public async Task<ApiResponse<StudentProfileResponse>> UploadProfilePhotoAsync(Guid userId, IFormFile photo)
    {
        var user = await _userService.GetByIdAsync(userId);
        if (user == null)
            return ApiResponse<StudentProfileResponse>.Fail("User not found.");

        var fileName = $"{userId}_{DateTime.UtcNow.Ticks}{Path.GetExtension(photo.FileName)}";
        var contentType = photo.ContentType ?? "image/jpeg";

        using var stream = photo.OpenReadStream();
        var photoUrl = await _fileStorage.UploadAsync(stream, fileName, contentType);

        await _userService.UpdateProfilePhotoAsync(userId, photoUrl);
        user.ProfilePhotoUrl = photoUrl;

        _logger.LogInformation("Profile photo uploaded to Bytescale for user {UserId}", userId);
        return ApiResponse<StudentProfileResponse>.Ok(MapToProfile(user), "Photo uploaded successfully.");
    }

    public async Task<ApiResponse<StudentDashboardResponse>> GetDashboardAsync(Guid userId)
    {
        var user = await _userService.GetByIdAsync(userId);
        if (user == null)
            return ApiResponse<StudentDashboardResponse>.Fail("User not found.");

        var coursesCount = await _registrationRepository.GetCountByUserIdAsync(userId);

        var dashboard = new StudentDashboardResponse
        {
            ApprovalStatus = user.ApprovalStatus,
            RegisteredCoursesCount = coursesCount,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email
        };

        // Find all registrations
        var registrations = await _registrationRepository.GetByUserIdAsync(userId);

        // Active/current course = first non-completed, or most recent if all completed
        var activeReg = registrations.FirstOrDefault(r => !r.IsCompleted)
                        ?? registrations.FirstOrDefault();

        if (activeReg != null)
        {
            var course = await _courseRepository.GetByIdAsync(activeReg.CourseId);
            if (course != null)
            {
                dashboard.CourseName = course.Name;
                dashboard.CourseDuration = course.Duration;
                dashboard.PaymentStatus = activeReg.PaymentStatus;
                dashboard.AmountDue = course.Fees - (activeReg.PaymentAmount ?? 0);
                dashboard.EnrolledDate = activeReg.CreatedAt;
                dashboard.ActiveRegistrationId = activeReg.Id;

                if (activeReg.PaymentStatus == "Paid")
                {
                    dashboard.CourseStatus = activeReg.IsCompleted ? "Completed" : "Ongoing";
                }
                else
                {
                    dashboard.CourseStatus = "Pending Payment";
                }
            }
        }

        // Populate completed courses (excluding the active one shown above)
        var completedRegs = registrations
            .Where(r => r.IsCompleted && r.Id != activeReg?.Id)
            .OrderByDescending(r => r.CompletedAt);

        foreach (var reg in completedRegs)
        {
            var course = await _courseRepository.GetByIdAsync(reg.CourseId);
            if (course != null)
            {
                dashboard.CompletedCourses.Add(new CompletedCourseDto
                {
                    RegistrationId = reg.Id,
                    CourseName = course.Name,
                    Duration = course.Duration,
                    CompletedAt = reg.CompletedAt,
                    EnrolledAt = reg.CreatedAt
                });
            }
        }

        return ApiResponse<StudentDashboardResponse>.Ok(dashboard);
    }

    public async Task<ApiResponse<List<StudentCourseResponse>>> GetRegisteredCoursesAsync(Guid userId)
    {
        var registrations = await _registrationRepository.GetByUserIdAsync(userId);
        var courses = new List<StudentCourseResponse>();

        foreach (var reg in registrations)
        {
            var course = await _courseRepository.GetByIdAsync(reg.CourseId);
            if (course != null)
            {
                courses.Add(new StudentCourseResponse
                {
                    RegistrationId = reg.Id,
                    CourseId = course.Id,
                    CourseName = course.Name,
                    CourseDescription = course.Description,
                    CourseFees = course.Fees,
                    CourseOfferPrice = course.OfferPrice,
                    Duration = course.Duration,
                    PaymentStatus = reg.PaymentStatus,
                    PaymentAmount = reg.PaymentAmount,
                    RegisteredAt = reg.CreatedAt
                });
            }
        }

        return ApiResponse<List<StudentCourseResponse>>.Ok(courses);
    }

    public async Task<ApiResponse<string>> RegisterForCourseAsync(Guid userId, CourseRegistrationRequest request)
    {
        _logger.LogInformation("Course registration attempt: User {UserId} for Course {CourseId}", userId, request.CourseId);

        var user = await _userService.GetByIdAsync(userId);
        if (user == null)
            return ApiResponse<string>.Fail("User not found.");

        var course = await _courseRepository.GetByIdAsync(request.CourseId);
        if (course == null)
            return ApiResponse<string>.Fail("Course not found.");

        if (!course.IsActive)
            return ApiResponse<string>.Fail("This course is not currently available.");

        var existing = await _registrationRepository.GetByUserAndCourseAsync(userId, request.CourseId);
        if (existing != null)
            return ApiResponse<string>.Fail("You are already registered for this course.");

        var registration = new CourseRegistration
        {
            UserId = userId,
            CourseId = request.CourseId,
            PaymentStatus = "Pending",
            IsActive = true
        };

        await _registrationRepository.CreateAsync(registration);
        _logger.LogInformation("Course registration created: {RegistrationId}", registration.Id);

        try
        {
            // await _emailService.SendCourseRegistrationEmailAsync(user.Email, user.FirstName, course.Name);
        }
        catch (Exception ex)
        {
            // _logger.LogWarning(ex, "Failed to send course registration email to {Email}, but registration was successful", user.Email);
        }

        return ApiResponse<string>.Ok("Course registration successful.");
    }

    private static StudentProfileResponse MapToProfile(User user)
    {
        return new StudentProfileResponse
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            DateOfBirth = user.DateOfBirth,
            Address = user.Address,
            Education = user.Education,
            ProfilePhotoUrl = user.ProfilePhotoUrl,
            Role = user.Role,
            ApprovalStatus = user.ApprovalStatus,
            EmailVerified = user.EmailVerified,
            CreatedAt = user.CreatedAt
        };
    }
}
