namespace StudentOnboardingApp.Models.Profile;

public class StudentProfileDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Address { get; set; }
    public string? Education { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public List<DocumentDto> Documents { get; set; } = [];
}
