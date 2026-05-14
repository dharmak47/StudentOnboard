using StudentOnboardingApp.ViewModels;

namespace StudentOnboardingApp.Views.Notifications;

public partial class NotificationsPage : ContentPage
{
    private readonly NotificationsViewModel _viewModel;

    public NotificationsPage(NotificationsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Hide entire list
        NotificationsList.Opacity = 0;
        NotificationsList.TranslationY = 10;

        await _viewModel.LoadNotificationsCommand.ExecuteAsync(null);

        // Fade in everything at once
        await Task.WhenAll(
            NotificationsList.FadeTo(1, 300, Easing.CubicOut),
            NotificationsList.TranslateTo(0, 0, 300, Easing.CubicOut)
        );
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        NotificationsList.Opacity = 0;
    }
}
