using StudentOnboardingApp.ViewModels.Auth;
using StudentOnboardingApp.Views.Faq;

namespace StudentOnboardingApp.Views.Auth;

public partial class SignupPage : ContentPage
{
    private readonly SignupViewModel _viewModel;
    private readonly IServiceProvider _serviceProvider;

    public SignupPage(SignupViewModel viewModel, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        _serviceProvider = serviceProvider;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        BotAvatar.Opacity = 0; BotAvatar.Scale = 0.5;
        BotBubble.Opacity = 0; BotBubble.TranslationX = 15;

        // Bot appears after page is ready
        await Task.WhenAll(
            BotAvatar.FadeTo(1, 250, Easing.CubicOut),
            BotAvatar.ScaleTo(1, 250, Easing.CubicOut),
            BotBubble.FadeTo(1, 250, Easing.CubicOut),
            BotBubble.TranslateTo(0, 0, 250, Easing.CubicOut)
        );

        // Auto-hide bubble
        await Task.Delay(4000);
        await BotBubble.FadeTo(0, 200, Easing.CubicOut);
    }

    private async void OnBotTapped(object sender, TappedEventArgs e)
    {
        await BotAvatar.ScaleTo(0.85, 80, Easing.CubicOut);
        await BotAvatar.ScaleTo(1.0, 150, Easing.SpringOut);
        await Navigation.PushAsync(_serviceProvider.GetRequiredService<FaqPage>());
    }
}
