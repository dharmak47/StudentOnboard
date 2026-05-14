namespace Student_Onboarding_Platform.Models.DTOs.Auth;

public class CheckApprovalStatusRequest
{
    public string Email { get; set; } = string.Empty;
}

public class CheckApprovalStatusResponse
{
    public string ApprovalStatus { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
