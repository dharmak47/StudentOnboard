namespace StudentOnboardingApp;

public static class Constants
{
    // API Configuration
    public const string ApiBaseUrl = "https://student-onboarding-api.onrender.com/api/";

    // Secure Storage Keys
    public const string AccessTokenKey = "access_token";
    public const string RefreshTokenKey = "refresh_token";
    public const string TokenExpiryKey = "token_expiry";
    public const string UserJsonKey = "user_json";
    public const string FcmTokenKey = "fcm_token";

    // Named HttpClients
    public const string PublicApiClient = "PublicApi";
    public const string AuthenticatedApiClient = "AuthenticatedApi";

    // Route Names
    public static class Routes
    {
        public const string Login = "login";
        public const string Signup = "signup";
        public const string OtpVerification = "otp-verification";
        public const string ForgotPassword = "forgot-password";
        public const string ResetPassword = "reset-password";
        public const string ApprovalWaiting = "approval-waiting";
        public const string OnboardingInstructions = "onboarding-instructions";
        public const string CourseDetail = "course-detail";
        public const string EditProfile = "edit-profile";
        public const string ChangePassword = "change-password";
    }
}
