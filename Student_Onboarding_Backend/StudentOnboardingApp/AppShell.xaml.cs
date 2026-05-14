using StudentOnboardingApp.Views.Auth;
using StudentOnboardingApp.Views.Courses;
using StudentOnboardingApp.Views.Onboarding;
using StudentOnboardingApp.Views.Profile;

namespace StudentOnboardingApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Auth routes
        Routing.RegisterRoute(Constants.Routes.Signup, typeof(SignupPage));
        Routing.RegisterRoute(Constants.Routes.OtpVerification, typeof(OtpVerificationPage));
        Routing.RegisterRoute(Constants.Routes.ForgotPassword, typeof(ForgotPasswordPage));
        Routing.RegisterRoute(Constants.Routes.ResetPassword, typeof(ResetPasswordPage));

        // Onboarding routes
        Routing.RegisterRoute(Constants.Routes.ApprovalWaiting, typeof(ApprovalWaitingPage));
        Routing.RegisterRoute(Constants.Routes.OnboardingInstructions, typeof(OnboardingInstructionsPage));

        // Detail routes
        Routing.RegisterRoute(Constants.Routes.CourseDetail, typeof(CourseDetailPage));
        Routing.RegisterRoute(Constants.Routes.EditProfile, typeof(EditProfilePage));
        Routing.RegisterRoute(Constants.Routes.ChangePassword, typeof(ChangePasswordPage));

    }

    /// <summary>
    /// Updates the badge count on the Alerts/Notifications tab.
    /// </summary>
    public void UpdateNotificationBadge(int count)
    {
        try
        {
            // Find the Alerts tab (index 3 in the TabBar: Dashboard=0, Courses=1, Profile=2, Alerts=3)
            if (Items.Count > 0 && Items[0] is TabBar tabBar && tabBar.Items.Count > 3)
            {
                // TabBar items are the ShellSections, but in flat TabBar they're ShellContents
                // We need to find the notifications tab
            }

            // Alternative: use the named element approach
            // The Alerts ShellContent is at position 3 in the TabBar
            var alertsTab = this.FindByName<ShellContent>("alertsTab");
            if (alertsTab != null)
            {
                // MAUI doesn't have built-in badge, but we can use Title to show count
                alertsTab.Title = count > 0 ? $"Alerts ({count})" : "Alerts";
            }
        }
        catch { /* ignore UI errors */ }
    }
}
