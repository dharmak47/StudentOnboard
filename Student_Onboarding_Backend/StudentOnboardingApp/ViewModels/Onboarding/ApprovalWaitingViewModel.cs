using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentOnboardingApp.Services.Interfaces;

namespace StudentOnboardingApp.ViewModels.Onboarding;

public partial class ApprovalWaitingViewModel : BaseViewModel
{
    private readonly IOnboardingService _onboardingService;
    private readonly ITokenStorageService _tokenStorage;
    private PeriodicTimer? _pollTimer;
    private CancellationTokenSource? _pollCts;

    public ApprovalWaitingViewModel(IOnboardingService onboardingService, ITokenStorageService tokenStorage)
    {
        _onboardingService = onboardingService;
        _tokenStorage = tokenStorage;
        Title = "Application Status";
    }

    [ObservableProperty]
    private string _statusMessage = "Your application is under review. Please wait for admin approval.";

    [ObservableProperty]
    private bool _isBlocked;

    [RelayCommand]
    private async Task StartPollingAsync()
    {
        _pollCts = new CancellationTokenSource();
        _pollTimer = new PeriodicTimer(TimeSpan.FromSeconds(10));

        // Check immediately
        await CheckStatusAsync();

        try
        {
            while (await _pollTimer.WaitForNextTickAsync(_pollCts.Token))
            {
                await CheckStatusAsync();
            }
        }
        catch (OperationCanceledException)
        {
            // Polling cancelled
        }
    }

    private async Task CheckStatusAsync()
    {
        var user = await _tokenStorage.GetUserAsync();
        if (user == null) return;

        var result = await _onboardingService.GetApprovalStatusAsync(user.Email);
        if (result.Success && result.Data != null)
        {
            switch (result.Data.ApprovalStatus)
            {
                case "Approved":
                    StopPolling();
                    // Start notification polling and go to dashboard
                    if (Application.Current is App app)
                        app.StartNotificationPolling();
                    await Shell.Current.GoToAsync("//main/dashboard");
                    break;
                case "Denied":
                    IsBlocked = true;
                    StatusMessage = result.Data.Message ?? "Your application has been denied. Please contact the administrator.";
                    StopPolling();
                    break;
                case "Pending":
                default:
                    StatusMessage = result.Data.Message ?? "Your application is under review. Please wait for admin approval.";
                    break;
            }
        }
    }

    public void StopPolling()
    {
        _pollCts?.Cancel();
        _pollTimer?.Dispose();
    }
}
