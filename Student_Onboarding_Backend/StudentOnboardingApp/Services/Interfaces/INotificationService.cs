using StudentOnboardingApp.Models.Common;
using StudentOnboardingApp.Models.Notification;

namespace StudentOnboardingApp.Services.Interfaces;

public interface INotificationService
{
    Task<ApiResponse<List<NotificationDto>>> GetNotificationsAsync();
    Task<ApiResponse<List<NotificationDto>>> GetAdminNotificationsAsync();
    Task<ApiResponse<string>> MarkAsReadAsync(Guid notificationId);
    Task<ApiResponse<string>> MarkAdminNotificationAsReadAsync(Guid notificationId);
    Task<ApiResponse<string>> RegisterDeviceTokenAsync(string fcmToken);
    Task<int> GetUnreadCountAsync();
    Task<ApiResponse<string>> SendNotificationAsync(string title, string message, List<Guid>? studentIds = null);
}
