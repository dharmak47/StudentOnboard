using StudentOnboardingApp.ViewModels;

namespace StudentOnboardingApp.Views.Notifications;

public partial class AdminNotificationsPage : ContentPage
{
    private readonly AdminNotificationsViewModel _viewModel;

    public AdminNotificationsPage(AdminNotificationsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadNotificationsCommand.ExecuteAsync(null);
    }
}
