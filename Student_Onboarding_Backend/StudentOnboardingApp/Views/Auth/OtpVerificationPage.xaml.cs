using StudentOnboardingApp.ViewModels.Auth;

namespace StudentOnboardingApp.Views.Auth;

public partial class OtpVerificationPage : ContentPage
{
    public OtpVerificationPage(OtpVerificationViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
