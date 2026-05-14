using System.Net.Http.Json;
using StudentOnboardingApp.Helpers;
using StudentOnboardingApp.Models.Common;
using StudentOnboardingApp.Models.Notification;
using StudentOnboardingApp.Services.Interfaces;

namespace StudentOnboardingApp.Services.Implementations;

public class NotificationService : INotificationService
{
    private readonly HttpClient _client;

    public NotificationService(IHttpClientFactory httpClientFactory)
    {
        _client = httpClientFactory.CreateClient(Constants.AuthenticatedApiClient);
    }

    public async Task<ApiResponse<List<NotificationDto>>> GetNotificationsAsync()
    {
        try
        {
            var response = await _client.GetAsync("student/notifications");
            return await HttpResponseParser.ParseAsync<List<NotificationDto>>(response);
        }
        catch
        {
            return new ApiResponse<List<NotificationDto>> { Success = false, Message = "Unable to load notifications." };
        }
    }

    public async Task<ApiResponse<List<NotificationDto>>> GetAdminNotificationsAsync()
    {
        try
        {
            var response = await _client.GetAsync("admin/notifications");
            return await HttpResponseParser.ParseAsync<List<NotificationDto>>(response);
        }
        catch
        {
            return new ApiResponse<List<NotificationDto>> { Success = false, Message = "Unable to load notifications." };
        }
    }

    public async Task<ApiResponse<string>> MarkAsReadAsync(Guid notificationId)
    {
        try
        {
            var response = await _client.PutAsync($"student/notifications/{notificationId}/read", null);
            return await HttpResponseParser.ParseAsync<string>(response);
        }
        catch
        {
            return new ApiResponse<string> { Success = false, Message = "Failed to mark notification as read." };
        }
    }

    public async Task<ApiResponse<string>> MarkAdminNotificationAsReadAsync(Guid notificationId)
    {
        try
        {
            var response = await _client.PutAsync($"admin/notifications/{notificationId}/read", null);
            return await HttpResponseParser.ParseAsync<string>(response);
        }
        catch
        {
            return new ApiResponse<string> { Success = false, Message = "Failed to mark notification as read." };
        }
    }

    public async Task<ApiResponse<string>> RegisterDeviceTokenAsync(string fcmToken)
    {
        try
        {
            var response = await _client.PostAsJsonAsync("student/notifications/register-device", new { Token = fcmToken });
            return await HttpResponseParser.ParseAsync<string>(response);
        }
        catch
        {
            return new ApiResponse<string> { Success = false, Message = "Failed to register device." };
        }
    }

    public async Task<int> GetUnreadCountAsync()
    {
        try
        {
            var result = await GetNotificationsAsync();
            if (result.Success && result.Data != null)
                return result.Data.Count(n => !n.IsRead);
            return 0;
        }
        catch
        {
            return 0;
        }
    }

    public async Task<ApiResponse<string>> SendNotificationAsync(string title, string message, List<Guid>? studentIds = null)
    {
        try
        {
            var request = new { Title = title, Message = message, StudentIds = studentIds };
            var response = await _client.PostAsJsonAsync("admin/notifications/send", request);
            return await HttpResponseParser.ParseAsync<string>(response);
        }
        catch
        {
            return new ApiResponse<string> { Success = false, Message = "Failed to send notification." };
        }
    }
}
