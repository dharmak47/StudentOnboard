using StudentOnboardingApp.ViewModels;

namespace StudentOnboardingApp.Views.Profile;

public partial class ProfilePage : ContentPage
{
    private readonly ProfileViewModel _viewModel;

    public ProfilePage(ProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        DobPicker.MaximumDate = DateTime.Today;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Hide entire page content
        ProfileContent.Opacity = 0;
        ProfileContent.TranslationY = 10;

        // Load data
        await _viewModel.LoadProfileCommand.ExecuteAsync(null);

        // Fade in the whole page at once
        await Task.WhenAll(
            ProfileContent.FadeTo(1, 300, Easing.CubicOut),
            ProfileContent.TranslateTo(0, 0, 300, Easing.CubicOut)
        );
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        ProfileContent.Opacity = 0;
    }
}
