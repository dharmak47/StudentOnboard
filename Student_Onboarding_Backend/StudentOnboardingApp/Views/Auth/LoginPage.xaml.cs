using StudentOnboardingApp.ViewModels.Auth;
using StudentOnboardingApp.Views.Faq;

namespace StudentOnboardingApp.Views.Auth;

public partial class LoginPage : ContentPage
{
    private readonly IServiceProvider _serviceProvider;

    public LoginPage(LoginViewModel viewModel, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _serviceProvider = serviceProvider;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Hide everything
        LogoIcon.Opacity = 0; LogoIcon.Scale = 0.7;
        HeaderLabel.Opacity = 0;
        FormCard.Opacity = 0; FormCard.TranslationY = 14;
        BotAvatar.Opacity = 0; BotAvatar.Scale = 0.5;
        BotBubble.Opacity = 0; BotBubble.TranslationX = 15;

        // All main content appears together
        await Task.WhenAll(
            LogoIcon.FadeTo(1, 300, Easing.CubicOut),
            LogoIcon.ScaleTo(1, 300, Easing.CubicOut),
            HeaderLabel.FadeTo(1, 300, Easing.CubicOut),
            FormCard.FadeTo(1, 300, Easing.CubicOut),
            FormCard.TranslateTo(0, 0, 300, Easing.CubicOut)
        );

        // Bot appears after page is settled
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
