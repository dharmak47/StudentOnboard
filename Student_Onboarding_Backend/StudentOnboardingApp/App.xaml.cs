using CommunityToolkit.Mvvm.Messaging;
using StudentOnboardingApp.Handlers;
using StudentOnboardingApp.Services.Implementations;
using StudentOnboardingApp.Services.Interfaces;

namespace StudentOnboardingApp;

public partial class App : Application
{
    private readonly ITokenStorageService _tokenStorage;
    private readonly NotificationPollingService _pollingService;
    private bool _isLoggingOut;
    private bool _startAuthenticated;

    public App(ITokenStorageService tokenStorage, NotificationPollingService pollingService)
    {
        InitializeComponent();
        _tokenStorage = tokenStorage;
        _pollingService = pollingService;

        // Check auth state synchronously before Shell is created
        // This prevents the login page flash
        _startAuthenticated = CheckAuthSync();

        // Listen for forced logout
        WeakReferenceMessenger.Default.Register<LogoutMessage>(this, async (_, _) =>
        {
            if (_isLoggingOut) return;
            _isLoggingOut = true;
            _pollingService.Stop();

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await _tokenStorage.ClearAllAsync();
                    await Shell.Current.GoToAsync("///auth/login");
                }
                catch { }
                finally { _isLoggingOut = false; }
            });
        });

        // Push notifications — show as popup alerts
        WeakReferenceMessenger.Default.Register<NewNotificationsMessage>(this, async (_, msg) =>
        {
            if (_isLoggingOut) return;
            foreach (var notification in msg.NewNotifications.Take(3))
            {
                try
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        var page = Shell.Current?.CurrentPage;
                        if (page == null) return;

                        var viewNotification = await page.DisplayAlert(
                            $"\ud83d\udd14 {notification.Title}",
                            notification.Body,
                            "View", "Dismiss");

                        if (viewNotification)
                        {
                            var user = await _tokenStorage.GetUserAsync();
                            var route = user?.Role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true
                                ? "//admin/admin-notifications"
                                : "//main/notifications";
                            await Shell.Current.GoToAsync(route);
                        }
                    });
                }
                catch { }
            }
        });

        // Badge count updates
        WeakReferenceMessenger.Default.Register<BadgeCountMessage>(this, (_, msg) =>
        {
            if (_isLoggingOut) return;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (Shell.Current is AppShell appShell)
                    appShell.UpdateNotificationBadge(msg.Count);
            });
        });
    }

    /// <summary>
    /// Synchronously check if tokens exist in SecureStorage.
    /// Used to decide the initial Shell route before UI renders.
    /// </summary>
    private bool CheckAuthSync()
    {
        try
        {
            var token = SecureStorage.Default.GetAsync(Constants.AccessTokenKey).Result;
            return !string.IsNullOrEmpty(token);
        }
        catch { return false; }
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var shell = new AppShell();

        // When a token exists, start Shell on the student dashboard tab directly
        // so the login page is never created or rendered — no flash at all.
        if (_startAuthenticated)
        {
            shell.CurrentItem = shell.Items[1]; // Items: 0=auth, 1=main, 2=admin
        }

        var window = new Window(shell);

        window.Created += async (_, _) =>
        {
            if (_startAuthenticated)
            {
                // Verify token is still valid
                var valid = await VerifyTokenAsync();
                if (valid)
                {
                    var user = await _tokenStorage.GetUserAsync();
                    if (user?.Role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        await Shell.Current.GoToAsync("//admin/dashboard");
                    }
                    // Student is already on //main/dashboard — no navigation needed
                    _pollingService.Start(TimeSpan.FromSeconds(15));
                    return;
                }
            }

            // Not authenticated or token invalid — go to login
            await _tokenStorage.ClearAllAsync();
            await Shell.Current.GoToAsync("///auth/login");
        };

        return window;
    }

    private async Task<bool> VerifyTokenAsync()
    {
        try
        {
            // First check if token exists and hasn't expired locally
            var isAuthenticated = await _tokenStorage.IsAuthenticatedAsync();
            if (!isAuthenticated) return false;

            var dashboardService = IPlatformApplication.Current?.Services.GetService<IDashboardService>();
            if (dashboardService == null) return false;

            var result = await dashboardService.GetDashboardAsync();
            return result.Success;
        }
        catch
        {
            return false;
        }
    }

    public void StartNotificationPolling()
    {
        _isLoggingOut = false;
        _pollingService.Start(TimeSpan.FromSeconds(15));
    }

    public void StopNotificationPolling()
    {
        _pollingService.Stop();
    }
}
