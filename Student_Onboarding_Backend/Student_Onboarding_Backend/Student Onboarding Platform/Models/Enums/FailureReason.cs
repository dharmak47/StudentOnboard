namespace Student_Onboarding_Platform.Models.Enums;

public enum FailureReason
{
    InvalidPassword,
    UserNotFound,
    AccountLocked,
    AccountInactive,
    EmailNotVerified,
    AccountDeleted,
    PendingApproval,
    AccountDenied
}
