namespace Student_Onboarding_Platform.Models.DTOs.Admin;

public class SendNotificationRequest
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public List<Guid>? StudentIds { get; set; }
}
