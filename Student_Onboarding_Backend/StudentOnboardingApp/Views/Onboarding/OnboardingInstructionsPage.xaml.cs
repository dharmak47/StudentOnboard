using StudentOnboardingApp.ViewModels.Onboarding;

namespace StudentOnboardingApp.Views.Onboarding;

public partial class OnboardingInstructionsPage : ContentPage
{
    private readonly OnboardingInstructionsViewModel _viewModel;

    public OnboardingInstructionsPage(OnboardingInstructionsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadInstructionsCommand.ExecuteAsync(null);
    }
}
