using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using StudentOnboardingApp.Models.Notification;
using StudentOnboardingApp.Services.Implementations;
using StudentOnboardingApp.Services.Interfaces;

namespace StudentOnboardingApp.ViewModels;

public partial class NotificationsViewModel : BaseViewModel
{
    private readonly INotificationService _notificationService;

    public NotificationsViewModel(INotificationService notificationService)
    {
        _notificationService = notificationService;
        Title = "Notifications";
    }

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private int _unreadCount;

    public ObservableCollection<NotificationDto> Notifications { get; } = [];

    [RelayCommand]
    private async Task LoadNotificationsAsync()
    {
        await ExecuteAsync(async () =>
        {
            var result = await _notificationService.GetNotificationsAsync();
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

        var result = await _notificationService.MarkAsReadAsync(notification.Id);
        if (result.Success)
        {
            notification.IsRead = true;
            // Refresh to update UI
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
            await _notificationService.MarkAsReadAsync(n.Id);
            n.IsRead = true;
        }
        // Refresh the whole list
        await LoadNotificationsAsync();
    }
}
