using Student_Onboarding_Platform.Data.Repositories.Interfaces;
using Student_Onboarding_Platform.Models.DTOs.Admin;
using Student_Onboarding_Platform.Models.DTOs.Common;
using Student_Onboarding_Platform.Models.Entities;
using Student_Onboarding_Platform.Models.Enums;
using Student_Onboarding_Platform.Services.Interfaces;

namespace Student_Onboarding_Platform.Services.Implementations;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUserService _userService;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository notificationRepository,
        IUserService userService,
        ILogger<NotificationService> logger)
    {
        _notificationRepository = notificationRepository;
        _userService = userService;
        _logger = logger;
    }

    public async Task NotifyAdminsOfNewRegistrationAsync(User student)
    {
        _logger.LogInformation("Creating notifications for admins about new student registration: {StudentId}", student.Id);

        var admins = await _userService.GetAdminUsersAsync();
        foreach (var admin in admins)
        {
            var notification = new Notification
            {
                UserId = admin.Id,
                Type = nameof(NotificationType.NewRegistration),
                Title = "New Student Registration",
                Message = $"{student.FirstName} {student.LastName} ({student.Email}) has verified their email and is pending approval.",
                ReferenceId = student.Id,
                IsRead = false
            };

            await _notificationRepository.CreateAsync(notification);
        }

        _logger.LogInformation("Notifications created for {Count} admin(s)", admins.Count());
    }

    public async Task NotifyAdminsOfCourseRegistrationAsync(User student, string courseName, Guid courseId)
    {
        _logger.LogInformation("Notifying admins of course registration by student {StudentId} for course {CourseName}", student.Id, courseName);

        var admins = await _userService.GetAdminUsersAsync();
        foreach (var admin in admins)
        {
            var notification = new Notification
            {
                UserId = admin.Id,
                Type = nameof(NotificationType.CourseRegistration),
                Title = "New Course Registration",
                Message = $"{student.FirstName} {student.LastName} has registered for \"{courseName}\". Payment is pending.",
                ReferenceId = courseId,
                IsRead = false
            };

            await _notificationRepository.CreateAsync(notification);
        }

        _logger.LogInformation("Course registration notifications created for {Count} admin(s)", admins.Count());
    }

    public async Task<ApiResponse<List<NotificationResponse>>> GetNotificationsAsync(Guid adminId)
    {
        var notifications = await _notificationRepository.GetByUserIdAsync(adminId);

        var response = notifications.Select(n => new NotificationResponse
        {
            Id = n.Id,
            Type = n.Type,
            Title = n.Title,
            Message = n.Message,
            ReferenceId = n.ReferenceId,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        }).ToList();

        return ApiResponse<List<NotificationResponse>>.Ok(response);
    }

    public async Task<ApiResponse<UnreadCountResponse>> GetUnreadCountAsync(Guid adminId)
    {
        var count = await _notificationRepository.GetUnreadCountAsync(adminId);
        return ApiResponse<UnreadCountResponse>.Ok(new UnreadCountResponse { Count = count });
    }

    public async Task<ApiResponse<string>> MarkAsReadAsync(Guid notificationId, Guid adminId)
    {
        await _notificationRepository.MarkAsReadAsync(notificationId);
        _logger.LogInformation("Notification {NotificationId} marked as read by admin {AdminId}", notificationId, adminId);
        return ApiResponse<string>.Ok("Notification marked as read.");
    }

    public async Task<ApiResponse<string>> SendToStudentsAsync(string title, string message, List<Guid>? studentIds = null)
    {
        var count = 0;

        if (studentIds != null && studentIds.Count > 0)
        {
            // Send to selected students by ID — no need to load all from DB
            foreach (var studentId in studentIds)
            {
                await CreateStudentNotificationAsync(studentId, "AdminMessage", title, message);
                count++;
            }
        }
        else
        {
            // Send to all approved students in batches to avoid OOM
            const int batchSize = 200;
            int offset = 0;
            while (true)
            {
                var batch = (await _userService.GetStudentsAsync(offset, batchSize, "Approved", null)).ToList();
                if (batch.Count == 0) break;

                foreach (var student in batch)
                {
                    await CreateStudentNotificationAsync(student.Id, "AdminMessage", title, message);
                    count++;
                }

                if (batch.Count < batchSize) break;
                offset += batchSize;
            }
        }

        _logger.LogInformation("Admin notification sent to {Count} student(s): {Title}", count, title);
        return ApiResponse<string>.Ok($"Notification sent to {count} student(s).");
    }

    // Student notification methods

    public async Task CreateStudentNotificationAsync(Guid studentId, string type, string title, string message, Guid? referenceId = null)
    {
        var notification = new Notification
        {
            UserId = studentId,
            Type = type,
            Title = title,
            Message = message,
            ReferenceId = referenceId,
            IsRead = false
        };

        await _notificationRepository.CreateAsync(notification);
        _logger.LogInformation("Student notification created for user {StudentId}: {Title}", studentId, title);
    }

    public async Task<ApiResponse<List<NotificationResponse>>> GetStudentNotificationsAsync(Guid studentId)
    {
        var notifications = await _notificationRepository.GetByUserIdAsync(studentId);

        var response = notifications.Select(n => new NotificationResponse
        {
            Id = n.Id,
            Type = n.Type,
            Title = n.Title,
            Message = n.Message,
            ReferenceId = n.ReferenceId,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        }).ToList();

        return ApiResponse<List<NotificationResponse>>.Ok(response);
    }

    public async Task<ApiResponse<string>> MarkStudentNotificationAsReadAsync(Guid notificationId, Guid studentId)
    {
        await _notificationRepository.MarkAsReadAsync(notificationId);
        _logger.LogInformation("Notification {NotificationId} marked as read by student {StudentId}", notificationId, studentId);
        return ApiResponse<string>.Ok("Notification marked as read.");
    }
}
