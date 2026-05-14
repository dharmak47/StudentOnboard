using StudentOnboardingApp.ViewModels.Onboarding;

namespace StudentOnboardingApp.Views.Onboarding;

public partial class ApprovalWaitingPage : ContentPage
{
    private readonly ApprovalWaitingViewModel _viewModel;

    public ApprovalWaitingPage(ApprovalWaitingViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.StartPollingCommand.ExecuteAsync(null);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.StopPolling();
    }
}
