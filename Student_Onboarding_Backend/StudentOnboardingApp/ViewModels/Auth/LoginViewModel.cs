using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentOnboardingApp.Models.Auth;
using StudentOnboardingApp.Services.Interfaces;

namespace StudentOnboardingApp.ViewModels.Auth;

public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _authService;

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService;
        Title = "Login";
    }

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _isPasswordVisible;

    [RelayCommand]
    private async Task LoginAsync()
    {
        await ExecuteAsync(async () =>
        {
            var request = new LoginRequest
            {
                Email = Email,
                Password = Password
            };

            var result = await _authService.LoginAsync(request);

            if (result.Success && result.Data != null)
            {
                if (!result.Data.User.EmailVerified)
                {
                    await Shell.Current.GoToAsync(
                        $"{Constants.Routes.OtpVerification}?email={Email}&otpType=EmailVerification");
                    return;
                }

                // Route based on user role
                var route = result.Data.User.Role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true
                    ? "//admin/dashboard"
                    : "//main/dashboard";
                await Shell.Current.GoToAsync(route);

                // Start notification polling
                if (Application.Current is App app)
                    app.StartNotificationPolling();
            }
            else
            {
                ErrorMessage = result.Message;
            }
        });
    }

    [RelayCommand]
    private async Task GoToSignupAsync()
    {
        await Shell.Current.GoToAsync(Constants.Routes.Signup);
    }

    [RelayCommand]
    private async Task GoToForgotPasswordAsync()
    {
        await Shell.Current.GoToAsync(Constants.Routes.ForgotPassword);
    }

    [RelayCommand]
    private void TogglePasswordVisibility()
    {
        IsPasswordVisible = !IsPasswordVisible;
    }
}
