using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentOnboardingApp.Models.Auth;
using StudentOnboardingApp.Models.Course;
using StudentOnboardingApp.Services.Interfaces;

namespace StudentOnboardingApp.ViewModels.Auth;

public partial class SignupViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly ICourseService _courseService;

    public SignupViewModel(IAuthService authService, ICourseService courseService)
    {
        _authService = authService;
        _courseService = courseService;
        Title = "Sign Up";
    }

    [ObservableProperty]
    private string _firstName = string.Empty;

    [ObservableProperty]
    private string _lastName = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _phoneNumber = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private bool _isPasswordVisible;

    [ObservableProperty]
    private CourseDto? _selectedCourse;

    public ObservableCollection<CourseDto> Courses { get; } = [];

    [RelayCommand]
    private async Task LoadCoursesAsync()
    {
        var result = await _courseService.GetCoursesAsync();
        if (result.Success && result.Data != null)
        {
            Courses.Clear();
            foreach (var course in result.Data)
                Courses.Add(course);
        }
    }

    [RelayCommand]
    private async Task SignupAsync()
    {
        await ExecuteAsync(async () =>
        {
            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Passwords do not match";
                return;
            }

            var request = new SignupRequest
            {
                FirstName = FirstName,
                LastName = LastName,
                Email = Email,
                PhoneNumber = PhoneNumber,
                Password = Password,
                ConfirmPassword = ConfirmPassword
            };

            var result = await _authService.SignupAsync(request);

            if (result.Success)
            {
                await Shell.Current.GoToAsync(
                    $"{Constants.Routes.OtpVerification}?email={Email}&otpType=EmailVerification");
            }
            else
            {
                ErrorMessage = result.Message;
            }
        });
    }

    [RelayCommand]
    private async Task GoToLoginAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
