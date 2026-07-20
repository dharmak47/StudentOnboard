using Microsoft.AspNetCore.Http;
using Student_Onboarding_Platform.Data.Repositories.Interfaces;
using Student_Onboarding_Platform.Models.DTOs.Admin;
using Student_Onboarding_Platform.Models.DTOs.Common;
using Student_Onboarding_Platform.Models.DTOs.Student;
using Student_Onboarding_Platform.Models.Entities;
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
    private readonly IPasswordHasher _passwordHasher;
    private readonly IInvoiceService _invoiceService;
    private readonly IStudentProgressService _progressService;
    private readonly ILogger<AdminService> _logger;

    public AdminService(
        IUserService userService,
        ICourseRepository courseRepository,
        ICourseRegistrationRepository registrationRepository,
        IEmailService emailService,
        INotificationService notificationService,
        ISessionService sessionService,
        IFileStorageService fileStorage,
        IPasswordHasher passwordHasher,
        IInvoiceService invoiceService,
        IStudentProgressService progressService,
        ILogger<AdminService> logger)
    {
        _userService = userService;
        _courseRepository = courseRepository;
        _registrationRepository = registrationRepository;
        _emailService = emailService;
        _notificationService = notificationService;
        _sessionService = sessionService;
        _fileStorage = fileStorage;
        _passwordHasher = passwordHasher;
        _invoiceService = invoiceService;
        _progressService = progressService;
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

        var items = new List<StudentListResponse>();

        foreach (var s in students)
        {
            var registrations = (await _registrationRepository.GetByUserIdAsync(s.Id)).ToList();
            var courses = new List<StudentCourseResponse>();

            _logger.LogInformation($"Fetching courses for student {s.Id}: Found {registrations.Count} registrations");

            foreach (var reg in registrations)
            {
                try
                {
                    var course = await _courseRepository.GetByIdAsync(reg.CourseId);
                    if (course != null)
                    {
                        // Fetch progress data for this registration
                        CourseProgressDto progressData = null;
                        try
                        {
                            var progressResult = await _progressService.GetProgressAsync(reg.Id);
                            progressData = progressResult.Data;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"Error fetching progress for registration {reg.Id}: {ex.Message}");
                            // Continue without progress data
                        }

                        var courseResponse = new StudentCourseResponse
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
                            RegisteredAt = reg.CreatedAt,
                            // Progress data
                            Status = progressData?.IsCompleted == true ? "Completed" : (reg.PaymentStatus == "Paid" ? "Active" : "Pending"),
                            ProgressPercentage = progressData?.ProgressPercentage ?? 0,
                            CompletedModules = progressData?.CompletedModules ?? 0,
                            TotalModules = progressData?.TotalModules ?? 0,
                            ExpectedCompletionDate = progressData?.ExpectedCompletionDate,
                            DaysRemaining = progressData?.DaysRemaining ?? 0,
                            IsCompleted = progressData?.IsCompleted ?? false
                        };

                        courses.Add(courseResponse);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error processing registration {reg.Id} for student {s.Id}: {ex.Message}");
                    // Continue to next registration
                }
            }

            var item = new StudentListResponse
            {
                Id = s.Id,
                FirstName = s.FirstName,
                LastName = s.LastName,
                Email = s.Email,
                PhoneNumber = s.PhoneNumber,
                ApprovalStatus = s.ApprovalStatus,
                EmailVerified = s.EmailVerified,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt,
                RegisteredCourses = courses
            };

            items.Add(item);
        }

        return ApiResponse<PaginatedResponse<StudentListResponse>>.Ok(new PaginatedResponse<StudentListResponse>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
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
                // Fetch progress data for this registration
                var progressResult = await _progressService.GetProgressAsync(reg.Id);
                var progressData = progressResult.Data;

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
                    RegisteredAt = reg.CreatedAt,
                    // Progress data
                    Status = progressData?.IsCompleted == true ? "Completed" : (reg.PaymentStatus == "Paid" ? "Active" : "Pending"),
                    ProgressPercentage = progressData?.ProgressPercentage ?? 0,
                    CompletedModules = progressData?.CompletedModules ?? 0,
                    TotalModules = progressData?.TotalModules ?? 0,
                    ExpectedCompletionDate = progressData?.ExpectedCompletionDate,
                    DaysRemaining = progressData?.DaysRemaining ?? 0,
                    IsCompleted = progressData?.IsCompleted ?? false
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

        // await _emailService.SendApprovalEmailAsync(student.Email, student.FirstName);

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

        // await _emailService.SendDenialEmailAsync(student.Email, student.FirstName, request.Reason);

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
                RegisteredAt = reg.CreatedAt,
                IsCompleted = reg.IsCompleted,
                CompletedAt = reg.CompletedAt
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

        // Auto-generate the invoice once a payment is fully settled (idempotent).
        if (string.Equals(request.PaymentStatus, "Paid", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                await _invoiceService.GetOrCreateForRegistrationAsync(registrationId, null, isAdmin: true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to auto-create invoice for registration {RegistrationId}", registrationId);
            }
        }

        _logger.LogInformation("Payment updated for registration {RegistrationId}: {Status}", registrationId, request.PaymentStatus);
        return ApiResponse<string>.Ok("Payment updated successfully.");
    }

    public async Task<ApiResponse<string>> CompleteCourseAsync(Guid registrationId)
    {
        var registration = await _registrationRepository.GetByIdAsync(registrationId);
        if (registration == null)
            return ApiResponse<string>.Fail("Registration not found.");

        if (registration.IsCompleted)
            return ApiResponse<string>.Fail("Course is already marked as completed.");

        await _registrationRepository.UpdateCompletionAsync(registrationId, DateTime.UtcNow, null, null, null);
        return ApiResponse<string>.Ok("Course marked as completed successfully.");
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

    public async Task<ApiResponse<string>> CreateUserAsync(CreateUserRequest request)
    {
        _logger.LogInformation("Admin creating new user: {Email} with role {Role}", request.Email, request.Role);

        var existingUser = await _userService.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            _logger.LogWarning("User creation failed: email {Email} already exists", request.Email);
            return ApiResponse<string>.Fail("An account with this email already exists.");
        }

        if (!string.IsNullOrEmpty(request.PhoneNumber))
        {
            var existingPhone = await _userService.GetByPhoneNumberAsync(request.PhoneNumber);
            if (existingPhone != null)
            {
                _logger.LogWarning("User creation failed: phone {Phone} already exists", request.PhoneNumber);
                return ApiResponse<string>.Fail("An account with this phone number already exists.");
            }
        }

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email.ToLowerInvariant(),
            PhoneNumber = request.PhoneNumber,
            PasswordHash = _passwordHasher.Hash(request.Password),
            EmailVerified = true,
            PhoneVerified = false,
            IsActive = true,
            IsDeleted = false,
            Role = request.Role,
            ApprovalStatus = nameof(ApprovalStatus.Approved)
        };

        await _userService.CreateAsync(user);
        _logger.LogInformation("User created successfully: {UserId} ({Email}) with role {Role}", user.Id, user.Email, user.Role);

        return ApiResponse<string>.Ok($"User {user.Email} created successfully with role {user.Role}.");
    }

    public async Task<ApiResponse<string>> ChangeUserPasswordAsync(Guid userId, AdminChangePasswordRequest request)
    {
        _logger.LogInformation("Admin changing password for user {UserId}", userId);

        var user = await _userService.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("Password change failed: user {UserId} not found", userId);
            return ApiResponse<string>.Fail("User not found.");
        }

        var newPasswordHash = _passwordHasher.Hash(request.NewPassword);
        await _userService.UpdatePasswordAsync(userId, newPasswordHash);

        await _sessionService.RevokeAllUserSessionsAsync(userId);
        _logger.LogInformation("All sessions revoked for user {UserId} — password changed", userId);

        return ApiResponse<string>.Ok("Password changed successfully. User will need to log in again.");
    }

    public async Task<ApiResponse<CompletionResponseDto>> MarkCourseCompleteAsync(UpdateCompletionRequest request, Guid adminId)
    {
        _logger.LogInformation("Admin {AdminId} marking course registration {RegistrationId} as complete", adminId, request.RegistrationId);

        var registration = await _registrationRepository.GetByIdAsync(request.RegistrationId);
        if (registration == null)
        {
            _logger.LogWarning("Course registration {RegistrationId} not found", request.RegistrationId);
            return ApiResponse<CompletionResponseDto>.Fail("Course registration not found.");
        }

        if (registration.IsCompleted)
        {
            _logger.LogWarning("Course registration {RegistrationId} is already completed", request.RegistrationId);
            return ApiResponse<CompletionResponseDto>.Fail("This course is already marked as completed.");
        }

        var completionDate = request.CompletionDate ?? DateTime.UtcNow;

        // Update completion status
        await _registrationRepository.UpdateCompletionAsync(
            registration.Id,
            completionDate,
            request.Grade,
            request.AdminNotes,
            adminId);

        // Get updated registration
        var updatedRegistration = await _registrationRepository.GetByIdAsync(registration.Id);

        // Get student and course info
        var student = await _userService.GetByIdAsync(registration.UserId);
        var course = await _courseRepository.GetByIdAsync(registration.CourseId);

        var response = new CompletionResponseDto
        {
            RegistrationId = registration.Id,
            UserId = registration.UserId,
            StudentName = student != null ? $"{student.FirstName} {student.LastName}" : "Unknown",
            StudentEmail = student?.Email ?? "Unknown",
            CourseId = registration.CourseId,
            CourseName = course?.Name ?? "Unknown",
            IsCompleted = true,
            CompletedAt = completionDate,
            Grade = request.Grade,
            AdminNotes = request.AdminNotes,
            CompletedByAdminId = adminId,
            ProgressPercentage = registration.ProgressPercentage,
            EnrolledDate = registration.CreatedAt,
            ExpectedCompletionDate = registration.ExpectedCompletionDate
        };

        _logger.LogInformation("Successfully marked course registration {RegistrationId} as complete", registration.Id);

        // Send notification to student
        try
        {
            await _notificationService.CreateStudentNotificationAsync(
                registration.UserId,
                "CourseCompletion",
                "Course Completed",
                $"Your course '{course?.Name}' has been marked as complete by an administrator.",
                registration.Id
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create completion notification for user {UserId}", registration.UserId);
        }

        return ApiResponse<CompletionResponseDto>.Ok(response);
    }

    public async Task<ApiResponse<PaginatedResponse<IncompleteRegistrationDto>>> GetIncompleteRegistrationsByCourseAsync(Guid courseId, int page, int pageSize)
    {
        _logger.LogInformation("Fetching incomplete registrations for course {CourseId}, page {Page}, pageSize {PageSize}", courseId, page, pageSize);

        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course == null)
        {
            _logger.LogWarning("Course {CourseId} not found", courseId);
            return ApiResponse<PaginatedResponse<IncompleteRegistrationDto>>.Fail("Course not found.");
        }

        var offset = (page - 1) * pageSize;
        var registrations = (await _registrationRepository.GetIncompleteRegistrationsByCourseAsync(courseId, offset, pageSize)).ToList();
        var totalCount = await _registrationRepository.GetIncompleteCountByCourseAsync(courseId);

        var items = new List<IncompleteRegistrationDto>();

        foreach (var reg in registrations)
        {
            var student = await _userService.GetByIdAsync(reg.UserId);

            items.Add(new IncompleteRegistrationDto
            {
                RegistrationId = reg.Id,
                UserId = reg.UserId,
                StudentName = student != null ? $"{student.FirstName} {student.LastName}" : "Unknown",
                StudentEmail = student?.Email ?? "Unknown",
                CourseId = courseId,
                CourseName = course.Name,
                ProgressPercentage = reg.ProgressPercentage,
                EnrolledDate = reg.CreatedAt,
                ExpectedCompletionDate = reg.ExpectedCompletionDate,
                IsAtRisk = reg.ProgressPercentage < 30,
                PaymentStatus = reg.PaymentStatus,
                CompletedModules = reg.CompletedModules > 0 ? new List<string> { $"{reg.CompletedModules}/{reg.TotalModules} modules" } : new List<string>()
            });
        }

        var response = new PaginatedResponse<IncompleteRegistrationDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        return ApiResponse<PaginatedResponse<IncompleteRegistrationDto>>.Ok(response);
    }
}
