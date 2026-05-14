using StudentOnboardingApp.ViewModels;
using StudentOnboardingApp.Views.Faq;

namespace StudentOnboardingApp.Views.Dashboard;

public partial class DashboardPage : ContentPage
{
    private readonly DashboardViewModel _viewModel;
    private readonly IServiceProvider _serviceProvider;

    public DashboardPage(DashboardViewModel viewModel, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        _serviceProvider = serviceProvider;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Hide the ENTIRE page content as one block
        PageContent.Opacity = 0;
        PageContent.TranslationY = 10;
        ProgressBarControl.Progress = 0;
        BotAvatar.Opacity = 0;
        BotAvatar.Scale = 0.5;
        BotBubble.Opacity = 0;
        BotBubble.TranslationX = 15;

        // Fetch data first (loading indicator is shown by the ViewModel's IsBusy)
        await _viewModel.LoadDashboardCommand.ExecuteAsync(null);

        // Everything is ready — fade in the ENTIRE page at once
        await Task.WhenAll(
            PageContent.FadeTo(1, 300, Easing.CubicOut),
            PageContent.TranslateTo(0, 0, 300, Easing.CubicOut)
        );

        // Progress bar fills after page is visible
        if (_viewModel.HasCourse && _viewModel.CourseProgress > 0)
        {
            if (_viewModel.IsCompleted)
                ProgressBarControl.ProgressColor = Color.FromArgb("#22C55E");

            await ProgressBarControl.ProgressTo(_viewModel.CourseProgress, 500, Easing.CubicOut);
        }

        // Bot appears after everything else is settled
        _ = AnimateBotAsync();
    }

    private async void OnFaqButtonTapped(object sender, TappedEventArgs e)
    {
        await BotAvatar.ScaleTo(0.85, 80, Easing.CubicOut);
        await BotAvatar.ScaleTo(1.0, 150, Easing.CubicOut);
        await Navigation.PushAsync(_serviceProvider.GetRequiredService<FaqPage>());
    }

    private async Task AnimateBotAsync()
    {
        var name = _viewModel.UserName;
        var hour = DateTime.Now.Hour;
        var timeGreet = hour < 12 ? "Good morning" : hour < 17 ? "Good afternoon" : "Good evening";
        BotMessage.Text = string.IsNullOrWhiteSpace(name) || name == "Student"
            ? $"{timeGreet}! Need help? Tap me!"
            : $"{timeGreet}, {name}! Need help? Tap me!";

        // Bot avatar + bubble appear together
        await Task.WhenAll(
            BotAvatar.FadeTo(1, 250, Easing.CubicOut),
            BotAvatar.ScaleTo(1, 250, Easing.CubicOut),
            BotBubble.FadeTo(1, 250, Easing.CubicOut),
            BotBubble.TranslateTo(0, 0, 250, Easing.CubicOut)
        );

        // Auto-hide bubble after 4 seconds
        await Task.Delay(4000);
        await BotBubble.FadeTo(0, 200, Easing.CubicOut);
    }
}
