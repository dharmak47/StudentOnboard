using Student_Onboarding_Platform.Models.DTOs.Student;

namespace Student_Onboarding_Platform.Models.DTOs.Admin;

public class StudentDetailResponse
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string ApprovalStatus { get; set; } = string.Empty;
    public string? DenialReason { get; set; }
    public bool EmailVerified { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public List<StudentCourseResponse> RegisteredCourses { get; set; } = new();
}
