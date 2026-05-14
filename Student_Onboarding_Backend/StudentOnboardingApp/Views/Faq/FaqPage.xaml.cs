using StudentOnboardingApp.Models.Faq;
using StudentOnboardingApp.Services.Interfaces;

namespace StudentOnboardingApp.Views.Faq;

public partial class FaqPage : ContentPage
{
    private readonly IFaqService? _faqService;

    private static readonly List<(string Question, string Answer)> FallbackFaqs = new()
    {
        ("How do I register for a course?",
         "Navigate to the Courses tab, select the course you're interested in, and tap the 'Register' button. Fill in the required details and submit."),

        ("What documents are required for onboarding?",
         "You will need a valid ID proof, academic transcripts, a passport-size photograph, and any prerequisite certification documents."),

        ("How can I check my registration status?",
         "Your registration status is shown on the Dashboard. You can also check the Courses tab for detailed enrollment information."),

        ("What are the payment options available?",
         "We accept payments via credit/debit cards, UPI, net banking, and bank transfers. Payment details are shared after registration approval."),

        ("How do I reset my password?",
         "Tap 'Forgot Password' on the login screen, enter your registered email, and follow the instructions sent to your inbox."),

        ("What is the refund policy?",
         "Refunds are available within 7 days of payment if the course has not started. After the course begins, partial refunds may be issued on a case-by-case basis."),

        ("How do I update my profile?",
         "Go to the Profile tab and tap the edit icon. You can update your personal details, contact information, and profile picture."),

        ("Who do I contact for technical issues?",
         "For technical issues, use the contact options below to call or email our support team. We typically respond within 24 hours."),
    };

    private const string PhoneNumber = "+919566112651";
    private const string EmailAddress = "support@eduadmin.com";

    public FaqPage(IFaqService faqService)
    {
        _faqService = faqService;
        InitializeComponent();
        _ = LoadFaqsAsync();
    }

    private async Task LoadFaqsAsync()
    {
        var faqItems = new List<(string Question, string Answer)>();

        try
        {
            if (_faqService != null)
            {
                var result = await _faqService.GetFaqsAsync();
                if (result.Success && result.Data != null && result.Data.Count > 0)
                {
                    foreach (var faq in result.Data.OrderBy(f => f.SortOrder))
                    {
                        faqItems.Add((faq.Question, faq.Answer));
                    }
                }
            }
        }
        catch
        {
            // Fallback to hardcoded FAQs on any error
        }

        // Use fallback if API returned nothing
        if (faqItems.Count == 0)
            faqItems.AddRange(FallbackFaqs);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            FaqList.Children.Clear();
            foreach (var (question, answer) in faqItems)
            {
                FaqList.Children.Add(CreateFaqCard(question, answer));
            }
        });
    }

    private static View CreateFaqCard(string question, string answer)
    {
        var answerLabel = new Label
        {
            Text = answer,
            FontSize = 13,
            TextColor = Color.FromArgb("#5A5A82"),
            LineHeight = 1.5,
            IsVisible = false,
            Margin = new Thickness(0, 10, 0, 0),
        };

        var chevron = new Label
        {
            Text = "\u25B6",
            FontSize = 12,
            TextColor = Color.FromArgb("#9898B8"),
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.End,
        };

        var questionLabel = new Label
        {
            Text = question,
            FontSize = 14,
            FontFamily = "OpenSansSemibold",
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#1A1A3E"),
            VerticalOptions = LayoutOptions.Center,
        };

        var headerGrid = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
            Children = { questionLabel, chevron },
        };
        Grid.SetColumn(chevron, 1);

        var stack = new VerticalStackLayout
        {
            Spacing = 0,
            Children = { headerGrid, answerLabel },
        };

        var border = new Border
        {
            BackgroundColor = Colors.White,
            StrokeThickness = 1,
            Stroke = new SolidColorBrush(Color.FromArgb("#E4E2F8")),
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 14 },
            Padding = new Thickness(18, 16),
            Content = stack,
            Shadow = new Shadow
            {
                Brush = new SolidColorBrush(Color.FromArgb("#145B5BD6")),
                Offset = new Point(0, 2),
                Radius = 8,
                Opacity = 0.08f,
            },
        };

        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += async (s, e) =>
        {
            var expanding = !answerLabel.IsVisible;
            chevron.Text = expanding ? "\u25BC" : "\u25B6";
            border.BackgroundColor = expanding
                ? Color.FromArgb("#FAFAFF")
                : Colors.White;

            if (expanding)
            {
                answerLabel.Opacity = 0;
                answerLabel.TranslationY = -6;
                answerLabel.IsVisible = true;
                await Task.WhenAll(
                    answerLabel.FadeTo(1, 180, Easing.CubicOut),
                    answerLabel.TranslateTo(0, 0, 200, Easing.CubicOut)
                );
            }
            else
            {
                await answerLabel.FadeTo(0, 120, Easing.CubicOut);
                answerLabel.IsVisible = false;
                answerLabel.TranslationY = 0;
            }
        };
        border.GestureRecognizers.Add(tapGesture);

        return border;
    }

    private async void OnCloseTapped(object sender, TappedEventArgs e)
    {
        try
        {
            await Navigation.PopAsync();
        }
        catch
        {
            await Shell.Current.GoToAsync("..");
        }
    }

    private async void OnPhoneTapped(object sender, TappedEventArgs e)
    {
        try
        {
            if (PhoneDialer.Default.IsSupported)
            {
                PhoneDialer.Default.Open(PhoneNumber);
            }
            else
            {
                // Fallback: use Launcher to open tel: URI
                await Launcher.Default.OpenAsync(new Uri($"tel:{PhoneNumber}"));
            }
        }
        catch
        {
            try
            {
                // Last resort fallback via Launcher
                await Launcher.Default.OpenAsync(new Uri($"tel:{PhoneNumber}"));
            }
            catch
            {
                await DisplayAlert("Unavailable", "Unable to make a phone call from this device. Please dial " + PhoneNumber + " manually.", "OK");
            }
        }
    }

    private async void OnEmailTapped(object sender, TappedEventArgs e)
    {
        try
        {
            if (Email.Default.IsComposeSupported)
            {
                var message = new EmailMessage
                {
                    To = new List<string> { EmailAddress },
                    Subject = "Support Request",
                };
                await Email.Default.ComposeAsync(message);
            }
            else
            {
                // Fallback: use Launcher to open mailto: URI
                await Launcher.Default.OpenAsync(new Uri($"mailto:{EmailAddress}?subject=Support%20Request"));
            }
        }
        catch
        {
            try
            {
                // Last resort fallback via Launcher
                await Launcher.Default.OpenAsync(new Uri($"mailto:{EmailAddress}?subject=Support%20Request"));
            }
            catch
            {
                await DisplayAlert("Unavailable", "Unable to open email. Please write to " + EmailAddress + " manually.", "OK");
            }
        }
    }
}
