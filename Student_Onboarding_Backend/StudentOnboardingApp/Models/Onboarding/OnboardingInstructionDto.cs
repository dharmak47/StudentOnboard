namespace StudentOnboardingApp.Models.Onboarding;

public class OnboardingInstructionDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int Order { get; set; }
}
