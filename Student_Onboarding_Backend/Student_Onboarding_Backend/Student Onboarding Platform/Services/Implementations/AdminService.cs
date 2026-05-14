using Microsoft.AspNetCore.Http;
using Student_Onboarding_Platform.Data.Repositories.Interfaces;
using Student_Onboarding_Platform.Models.DTOs.Admin;
using Student_Onboarding_Platform.Models.DTOs.Common;
using Student_Onboarding_Platform.Models.DTOs.Student;
using Student_Onboarding_Platform.Models.Enums;
using Student_Onboarding_Platform.Services.Interfaces;

namespace Student_Onboarding_Platform.Services.Implementations;

public class AdminService : IAdminService
{
    private readonly IUserService _userService;
    private readonly ICourseRepository _courseRepository;
    private readonly ICourseRegistrationRepository _registrationRepository;
    private readonly IEmailService _emailService;
    private readonly INotificationService _notificationService;
    private readonly ISessionService _sessionService;
    private readonly IFileStorageService _fileStorage;
    private readonly ILogger<AdminService> _logger;

    public AdminService(
        IUserService userService,
        ICourseRepository courseRepository,
        ICourseRegistrationRepository registrationRepository,
        IEmailService emailService,
        INotificationService notificationService,
        ISessionService sessionService,
        IFileStorageService fileStorage,
        ILogger<AdminService> logger)
    {
        _userService = userService;
        _courseRepository = courseRepository;
        _registrationRepository = registrationRepository;
        _emailService = emailService;
        _notificationService = notificationService;
        _sessionService = sessionService;
        _fileStorage = fileStorage;
        _logger = logger;
    }

    public async Task<ApiResponse<AdminDashboardResponse>> GetDashboardAsync()
    {
        var totalStudents = await _userService.GetStudentsCountAsync(null, null);
        var pendingApprovals = await _userService.GetStudentsCountAsync(nameof(ApprovalStatus.Pending), null);
        var approvedStudents = await _userService.GetStudentsCountAsync(nameof(ApprovalStatus.Approved), null);
        var deniedStudents = await _userService.GetStudentsCountAsync(nameof(ApprovalStatus.Denied), null);
        var totalCourses = (await _courseRepository.GetActiveCoursesAsync()).Count();
        var totalRegistrations = await _registrationRepository.GetAllCountAsync();

        var dashboard = new AdminDashboardResponse
        {
            TotalStudents = totalStudents,
            PendingApprovals = pendingApprovals,
            ApprovedStudents = approvedStudents,
            DeniedStudents = deniedStudents,
            TotalCourses = totalCourses,
            TotalRegistrations = totalRegistrations
        };

        return ApiResponse<AdminDashboardResponse>.Ok(dashboard);
    }

    public async Task<ApiResponse<PaginatedResponse<StudentListResponse>>> GetStudentsAsync(int page, int pageSize, string? status, string? search)
    {
        var offset = (page - 1) * pageSize;
        var students = await _userService.GetStudentsAsync(offset, pageSize, status, search);
        var totalCount = await _userService.GetStudentsCountAsync(status, search);

        var items = students.Select(s => new StudentListResponse
        {
            Id = s.Id,
            FirstName = s.FirstName,
            LastName = s.LastName,
            Email = s.Email,
            PhoneNumber = s.PhoneNumber,
            ApprovalStatus = s.ApprovalStatus,
            EmailVerified = s.EmailVerified,
            IsActive = s.IsActive,
            CreatedAt = s.CreatedAt
        }).ToList();

        var response = new PaginatedResponse<StudentListResponse>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };

        return ApiResponse<PaginatedResponse<StudentListResponse>>.Ok(response);
    }

    public async Task<ApiResponse<StudentDetailResponse>> GetStudentByIdAsync(Guid studentId)
    {
        var student = await _userService.GetByIdAsync(studentId);
        if (student == null)
            return ApiResponse<StudentDetailResponse>.Fail("Student not found.");

        var registrations = await _registrationRepository.GetByUserIdAsync(studentId);
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

        var detail = new StudentDetailResponse
        {
            Id = student.Id,
            FirstName = student.FirstName,
            LastName = student.LastName,
            Email = student.Email,
            PhoneNumber = student.PhoneNumber,
            ApprovalStatus = student.ApprovalStatus,
            DenialReason = student.DenialReason,
            EmailVerified = student.EmailVerified,
            IsActive = student.IsActive,
            CreatedAt = student.CreatedAt,
            LastLoginAt = student.LastLoginAt,
            RegisteredCourses = courses
        };

        return ApiResponse<StudentDetailResponse>.Ok(detail);
    }

    public async Task<ApiResponse<string>> UpdateStudentAsync(Guid studentId, UpdateStudentRequest request)
    {
        var student = await _userService.GetByIdAsync(studentId);
        if (student == null)
            return ApiResponse<string>.Fail("Student not found.");

        _logger.LogInformation("Updating student {StudentId} IsActive to {IsActive}", studentId, request.IsActive);
        await _userService.UpdateIsActiveAsync(studentId, request.IsActive);

        // If deactivating, revoke all sessions to force immediate logout
        if (!request.IsActive)
        {
            await _sessionService.RevokeAllUserSessionsAsync(studentId);
            _logger.LogInformation("All sessions revoked for deactivated student {StudentId}", studentId);
        }

        return ApiResponse<string>.Ok("Student updated successfully.");
    }

    public async Task<ApiResponse<string>> ApproveStudentAsync(Guid studentId, Guid adminId)
    {
        _logger.LogInformation("Admin {AdminId} approving student {StudentId}", adminId, studentId);

        var student = await _userService.GetByIdAsync(studentId);
        if (student == null)
            return ApiResponse<string>.Fail("Student not found.");

        if (student.ApprovalStatus == nameof(ApprovalStatus.Approved))
            return ApiResponse<string>.Fail("Student is already approved.");

        await _userService.UpdateApprovalStatusAsync(studentId, nameof(ApprovalStatus.Approved), adminId, null);

        await _emailService.SendApprovalEmailAsync(student.Email, student.FirstName);

        await _notificationService.CreateStudentNotificationAsync(
            studentId,
            "StudentApproved",
            "Account Approved!",
            $"Congratulations {student.FirstName}! Your account has been approved. You can now access all platform features and register for courses.");

        _logger.LogInformation("Student {StudentId} approved by admin {AdminId}", studentId, adminId);
        return ApiResponse<string>.Ok("Student approved successfully.");
    }

    public async Task<ApiResponse<string>> DenyStudentAsync(Guid studentId, Guid adminId, DenyStudentRequest request)
    {
        _logger.LogInformation("Admin {AdminId} denying student {StudentId}", adminId, studentId);

        var student = await _userService.GetByIdAsync(studentId);
        if (student == null)
            return ApiResponse<string>.Fail("Student not found.");

        if (student.ApprovalStatus == nameof(ApprovalStatus.Denied))
            return ApiResponse<string>.Fail("Student is already denied.");

        await _userService.UpdateApprovalStatusAsync(studentId, nameof(ApprovalStatus.Denied), adminId, request.Reason);

        // Revoke all active sessions to force immediate logout
        await _sessionService.RevokeAllUserSessionsAsync(studentId);
        _logger.LogInformation("All sessions revoked for denied student {StudentId}", studentId);

        await _emailService.SendDenialEmailAsync(student.Email, student.FirstName, request.Reason);

        await _notificationService.CreateStudentNotificationAsync(
            studentId,
            "StudentDenied",
            "Account Application Update",
            $"Hi {student.FirstName}, your account application has been reviewed. Reason: {request.Reason ?? "Not specified"}. Please contact support for assistance.");

        _logger.LogInformation("Student {StudentId} denied by admin {AdminId}", studentId, adminId);
        return ApiResponse<string>.Ok("Student denied successfully.");
    }

    public async Task<ApiResponse<PaginatedResponse<CourseRegistrationListResponse>>> GetCourseRegistrationsAsync(int page, int pageSize)
    {
        var offset = (page - 1) * pageSize;
        var registrations = await _registrationRepository.GetAllAsync(offset, pageSize);
        var totalCount = await _registrationRepository.GetAllCountAsync();

        var items = new List<CourseRegistrationListResponse>();
        foreach (var reg in registrations)
        {
            var student = await _userService.GetByIdAsync(reg.UserId);
            var course = await _courseRepository.GetByIdAsync(reg.CourseId);

            items.Add(new CourseRegistrationListResponse
            {
                Id = reg.Id,
                UserId = reg.UserId,
                StudentName = student != null ? $"{student.FirstName} {student.LastName}" : "Unknown",
                StudentEmail = student?.Email ?? "Unknown",
                CourseId = reg.CourseId,
                CourseName = course?.Name ?? "Unknown",
                PaymentStatus = reg.PaymentStatus,
                PaymentAmount = reg.PaymentAmount,
                PaymentDate = reg.PaymentDate,
                Notes = reg.Notes,
                RegisteredAt = reg.CreatedAt
            });
        }

        var response = new PaginatedResponse<CourseRegistrationListResponse>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };

        return ApiResponse<PaginatedResponse<CourseRegistrationListResponse>>.Ok(response);
    }

    public async Task<ApiResponse<string>> UpdatePaymentAsync(Guid registrationId, UpdatePaymentRequest request)
    {
        _logger.LogInformation("Updating payment for registration {RegistrationId}", registrationId);

        var registration = await _registrationRepository.GetByIdAsync(registrationId);
        if (registration == null)
            return ApiResponse<string>.Fail("Course registration not found.");

        await _registrationRepository.UpdatePaymentAsync(
            registrationId, request.PaymentStatus, request.PaymentAmount, request.Notes);

        var course = await _courseRepository.GetByIdAsync(registration.CourseId);
        await _notificationService.CreateStudentNotificationAsync(
            registration.UserId,
            "PaymentUpdate",
            "Payment Status Updated",
            $"Your payment for {course?.Name ?? "your course"} has been updated to: {request.PaymentStatus}.",
            registrationId);

        _logger.LogInformation("Payment updated for registration {RegistrationId}: {Status}", registrationId, request.PaymentStatus);
        return ApiResponse<string>.Ok("Payment updated successfully.");
    }

    public async Task<ApiResponse<string>> UploadProfilePhotoAsync(Guid adminId, IFormFile photo)
    {
        var user = await _userService.GetByIdAsync(adminId);
        if (user == null)
            return ApiResponse<string>.Fail("Admin not found.");

        var fileName = $"{adminId}_{DateTime.UtcNow.Ticks}{Path.GetExtension(photo.FileName)}";
        var contentType = photo.ContentType ?? "image/jpeg";

        using var stream = photo.OpenReadStream();
        var photoUrl = await _fileStorage.UploadAsync(stream, fileName, contentType);

        await _userService.UpdateProfilePhotoAsync(adminId, photoUrl);

        _logger.LogInformation("Profile photo uploaded for admin {AdminId}", adminId);
        return ApiResponse<string>.Ok(photoUrl, "Profile photo uploaded successfully.");
    }
}
