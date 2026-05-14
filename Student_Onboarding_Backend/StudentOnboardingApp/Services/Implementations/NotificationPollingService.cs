using CommunityToolkit.Mvvm.Messaging;
using StudentOnboardingApp.Models.Notification;
using StudentOnboardingApp.Services.Interfaces;

namespace StudentOnboardingApp.Services.Implementations;

/// <summary>
/// Message sent when new unread notifications are detected.
/// </summary>
public class NewNotificationsMessage
{
    public int UnreadCount { get; set; }
    public List<NotificationDto> NewNotifications { get; set; } = [];
}

/// <summary>
/// Message sent to update the badge count on the Alerts tab.
/// </summary>
public class BadgeCountMessage
{
    public int Count { get; set; }
}

public class NotificationPollingService : IDisposable
{
    private readonly INotificationService _notificationService;
    private readonly ITokenStorageService _tokenStorage;
    private PeriodicTimer? _timer;
    private CancellationTokenSource? _cts;
    private HashSet<Guid> _knownNotificationIds = [];
    private bool _isFirstPoll = true;

    public NotificationPollingService(
        INotificationService notificationService,
        ITokenStorageService tokenStorage)
    {
        _notificationService = notificationService;
        _tokenStorage = tokenStorage;
    }

    public void Start(TimeSpan interval)
    {
        Stop();
        _cts = new CancellationTokenSource();
        _timer = new PeriodicTimer(interval);
        _isFirstPoll = true;
        _knownNotificationIds.Clear();
        _ = PollLoopAsync(_cts.Token);
    }

    public void Stop()
    {
        _cts?.Cancel();
        _timer?.Dispose();
        _timer = null;
        _cts = null;
    }

    private async Task PollLoopAsync(CancellationToken ct)
    {
        // Initial poll immediately
        await PollOnceAsync(ct);

        while (!ct.IsCancellationRequested)
        {
            try
            {
                if (!await _timer!.WaitForNextTickAsync(ct))
                    break;
                await PollOnceAsync(ct);
            }
            catch (OperationCanceledException) { break; }
            catch { /* silently continue */ }
        }
    }

    private async Task PollOnceAsync(CancellationToken ct)
    {
        try
        {
            var isAuth = await _tokenStorage.IsAuthenticatedAsync();
            if (!isAuth) return;

            var result = await _notificationService.GetNotificationsAsync();
            if (!result.Success || result.Data == null) return;

            var allNotifications = result.Data;
            var unreadCount = allNotifications.Count(n => !n.IsRead);

            // Send badge update
            WeakReferenceMessenger.Default.Send(new BadgeCountMessage { Count = unreadCount });

            if (_isFirstPoll)
            {
                // On first poll, just record known IDs — don't show toasts
                _knownNotificationIds = new HashSet<Guid>(allNotifications.Select(n => n.Id));
                _isFirstPoll = false;
                return;
            }

            // Detect new notifications
            var newNotifications = allNotifications
                .Where(n => !_knownNotificationIds.Contains(n.Id))
                .ToList();

            if (newNotifications.Count > 0)
            {
                // Update known set
                foreach (var n in newNotifications)
                    _knownNotificationIds.Add(n.Id);

                // Broadcast new notifications for toast display
                WeakReferenceMessenger.Default.Send(new NewNotificationsMessage
                {
                    UnreadCount = unreadCount,
                    NewNotifications = newNotifications
                });
            }
        }
        catch { /* silently fail */ }
    }

    public void Dispose()
    {
        Stop();
    }
}
