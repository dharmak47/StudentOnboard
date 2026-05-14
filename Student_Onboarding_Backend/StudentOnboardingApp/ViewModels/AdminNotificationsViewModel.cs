using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using StudentOnboardingApp.Models.Notification;
using StudentOnboardingApp.Services.Implementations;
using StudentOnboardingApp.Services.Interfaces;

namespace StudentOnboardingApp.ViewModels;

public partial class AdminNotificationsViewModel : BaseViewModel
{
    private readonly INotificationService _notificationService;

    public AdminNotificationsViewModel(INotificationService notificationService)
    {
        _notificationService = notificationService;
        Title = "Notifications";
    }

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private int _unreadCount;

    [ObservableProperty]
    private string? _successMessage;

    // Send notification form fields
    [ObservableProperty]
    private string _notificationTitle = string.Empty;

    [ObservableProperty]
    private string _notificationMessage = string.Empty;

    [ObservableProperty]
    private bool _isSending;

    [ObservableProperty]
    private bool _showSendForm;

    public ObservableCollection<NotificationDto> Notifications { get; } = [];

    [RelayCommand]
    private void ToggleSendForm()
    {
        ShowSendForm = !ShowSendForm;
        if (!ShowSendForm)
        {
            // Clear form when closing
            NotificationTitle = string.Empty;
            NotificationMessage = string.Empty;
            ErrorMessage = null;
            SuccessMessage = null;
        }
    }

    [RelayCommand]
    private async Task SendNotificationAsync()
    {
        if (string.IsNullOrWhiteSpace(NotificationTitle))
        {
            ErrorMessage = "Please enter a notification title.";
            return;
        }

        if (string.IsNullOrWhiteSpace(NotificationMessage))
        {
            ErrorMessage = "Please enter a notification message.";
            return;
        }

        try
        {
            IsSending = true;
            ErrorMessage = null;
            SuccessMessage = null;

            var result = await _notificationService.SendNotificationAsync(
                NotificationTitle.Trim(),
                NotificationMessage.Trim());

            if (result.Success)
            {
                SuccessMessage = "Notification sent successfully!";
                NotificationTitle = string.Empty;
                NotificationMessage = string.Empty;
                ShowSendForm = false;

                // Refresh the notification list
                await LoadNotificationsAsync();
            }
            else
            {
                ErrorMessage = result.Message ?? "Failed to send notification.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsSending = false;
        }
    }

    [RelayCommand]
    private async Task LoadNotificationsAsync()
    {
        await ExecuteAsync(async () =>
        {
            var result = await _notificationService.GetAdminNotificationsAsync();
            if (result.Success && result.Data != null)
            {
                Notifications.Clear();
                foreach (var notification in result.Data.OrderByDescending(n => n.CreatedAt))
                    Notifications.Add(notification);

                UnreadCount = result.Data.Count(n => !n.IsRead);
                WeakReferenceMessenger.Default.Send(new BadgeCountMessage { Count = UnreadCount });
            }
            else if (result.Message?.Contains("Session expired", StringComparison.OrdinalIgnoreCase) == true
                     || result.Message?.Contains("401", StringComparison.OrdinalIgnoreCase) == true)
            {
                // Token expired — handled by App.xaml.cs LogoutMessage
            }
            else
            {
                ErrorMessage = result.Message;
            }
        });
        IsRefreshing = false;
    }

    [RelayCommand]
    private async Task MarkAsReadAsync(NotificationDto notification)
    {
        if (notification.IsRead) return;

        var result = await _notificationService.MarkAdminNotificationAsReadAsync(notification.Id);
        if (result.Success)
        {
            notification.IsRead = true;
            var index = Notifications.IndexOf(notification);
            if (index >= 0)
            {
                Notifications.RemoveAt(index);
                Notifications.Insert(index, notification);
            }

            UnreadCount = Notifications.Count(n => !n.IsRead);
            WeakReferenceMessenger.Default.Send(new BadgeCountMessage { Count = UnreadCount });
        }
    }

    [RelayCommand]
    private async Task MarkAllAsReadAsync()
    {
        var unread = Notifications.Where(n => !n.IsRead).ToList();
        foreach (var n in unread)
        {
            await _notificationService.MarkAdminNotificationAsReadAsync(n.Id);
            n.IsRead = true;
        }
        await LoadNotificationsAsync();
    }
}
