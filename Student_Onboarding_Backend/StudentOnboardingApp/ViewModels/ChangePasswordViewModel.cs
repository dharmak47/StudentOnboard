using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentOnboardingApp.Models.Auth;
using StudentOnboardingApp.Services.Interfaces;

namespace StudentOnboardingApp.ViewModels;

public partial class ChangePasswordViewModel : BaseViewModel
{
    private readonly IAuthService _authService;

    public ChangePasswordViewModel(IAuthService authService)
    {
        _authService = authService;
        Title = "Change Password";
    }

    [ObservableProperty]
    private string _currentPassword = string.Empty;

    [ObservableProperty]
    private string _newPassword = string.Empty;

    [ObservableProperty]
    private string _confirmNewPassword = string.Empty;

    [RelayCommand]
    private async Task ChangePasswordAsync()
    {
        await ExecuteAsync(async () =>
        {
            if (NewPassword != ConfirmNewPassword)
            {
                ErrorMessage = "Passwords do not match";
                return;
            }

            var request = new ChangePasswordRequest
            {
                CurrentPassword = CurrentPassword,
                NewPassword = NewPassword,
                ConfirmNewPassword = ConfirmNewPassword
            };

            var result = await _authService.ChangePasswordAsync(request);

            if (result.Success)
            {
                await Shell.Current.DisplayAlert("Success", "Password changed successfully.", "OK");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                ErrorMessage = result.Message;
            }
        });
    }
}
