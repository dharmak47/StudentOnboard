using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentOnboardingApp.Models.Auth;
using StudentOnboardingApp.Services.Interfaces;

namespace StudentOnboardingApp.ViewModels.Auth;

[QueryProperty(nameof(Email), "email")]
[QueryProperty(nameof(OtpType), "otpType")]
public partial class OtpVerificationViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private CancellationTokenSource? _timerCts;

    public OtpVerificationViewModel(IAuthService authService)
    {
        _authService = authService;
        Title = "Verify OTP";
    }

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _otpType = string.Empty;

    [ObservableProperty]
    private string _otpCode = string.Empty;

    [ObservableProperty]
    private int _resendCountdown;

    [ObservableProperty]
    private bool _canResend = true;

    [RelayCommand]
    private async Task VerifyAsync()
    {
        await ExecuteAsync(async () =>
        {
            var request = new VerifyOtpRequest
            {
                Email = Email,
                OtpCode = OtpCode,
                OtpType = OtpType
            };

            var result = await _authService.VerifyOtpAsync(request);

            if (result.Success)
            {
                if (OtpType == "EmailVerification")
                {
                    await Shell.Current.GoToAsync(Constants.Routes.ApprovalWaiting);
                }
                else if (OtpType == "PasswordReset")
                {
                    await Shell.Current.GoToAsync(
                        $"{Constants.Routes.ResetPassword}?email={Email}&otpCode={OtpCode}");
                }
            }
            else
            {
                ErrorMessage = result.Message;
            }
        });
    }

    [RelayCommand]
    private async Task ResendOtpAsync()
    {
        if (!CanResend) return;

        await ExecuteAsync(async () =>
        {
            var request = new ResendOtpRequest
            {
                Email = Email,
                OtpType = OtpType
            };

            var result = await _authService.ResendOtpAsync(request);

            if (result.Success)
            {
                StartResendCooldown();
            }
            else
            {
                ErrorMessage = result.Message;
            }
        });
    }

    private void StartResendCooldown()
    {
        CanResend = false;
        ResendCountdown = 60;
        _timerCts?.Cancel();
        _timerCts = new CancellationTokenSource();

        _ = Task.Run(async () =>
        {
            while (ResendCountdown > 0 && !_timerCts.Token.IsCancellationRequested)
            {
                await Task.Delay(1000, _timerCts.Token);
                MainThread.BeginInvokeOnMainThread(() => ResendCountdown--);
            }
            MainThread.BeginInvokeOnMainThread(() => CanResend = true);
        }, _timerCts.Token);
    }
}
