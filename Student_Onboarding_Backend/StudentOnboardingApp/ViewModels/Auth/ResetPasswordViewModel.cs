using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentOnboardingApp.Models.Auth;
using StudentOnboardingApp.Services.Interfaces;

namespace StudentOnboardingApp.ViewModels.Auth;

[QueryProperty(nameof(Email), "email")]
[QueryProperty(nameof(OtpCode), "otpCode")]
public partial class ResetPasswordViewModel : BaseViewModel
{
    private readonly IAuthService _authService;

    public ResetPasswordViewModel(IAuthService authService)
    {
        _authService = authService;
        Title = "Reset Password";
    }

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _otpCode = string.Empty;

    [ObservableProperty]
    private string _newPassword = string.Empty;

    [ObservableProperty]
    private string _confirmNewPassword = string.Empty;

    [RelayCommand]
    private async Task ResetPasswordAsync()
    {
        await ExecuteAsync(async () =>
        {
            if (NewPassword != ConfirmNewPassword)
            {
                ErrorMessage = "Passwords do not match";
                return;
            }

            var request = new ResetPasswordRequest
            {
                Email = Email,
                OtpCode = OtpCode,
                NewPassword = NewPassword,
                ConfirmNewPassword = ConfirmNewPassword
            };

            var result = await _authService.ResetPasswordAsync(request);

            if (result.Success)
            {
                await Shell.Current.DisplayAlert("Success", "Password reset successfully. Please login.", "OK");
                await Shell.Current.GoToAsync("///auth/login");
            }
            else
            {
                ErrorMessage = result.Message;
            }
        });
    }
}
