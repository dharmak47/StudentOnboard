using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentOnboardingApp.Models.Auth;
using StudentOnboardingApp.Services.Interfaces;

namespace StudentOnboardingApp.ViewModels.Auth;

public partial class ForgotPasswordViewModel : BaseViewModel
{
    private readonly IAuthService _authService;

    public ForgotPasswordViewModel(IAuthService authService)
    {
        _authService = authService;
        Title = "Forgot Password";
    }

    [ObservableProperty]
    private string _email = string.Empty;

    [RelayCommand]
    private async Task SendResetOtpAsync()
    {
        await ExecuteAsync(async () =>
        {
            var request = new ForgotPasswordRequest { Email = Email };
            var result = await _authService.ForgotPasswordAsync(request);

            if (result.Success)
            {
                await Shell.Current.GoToAsync(
                    $"{Constants.Routes.OtpVerification}?email={Email}&otpType=PasswordReset");
            }
            else
            {
                ErrorMessage = result.Message;
            }
        });
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
