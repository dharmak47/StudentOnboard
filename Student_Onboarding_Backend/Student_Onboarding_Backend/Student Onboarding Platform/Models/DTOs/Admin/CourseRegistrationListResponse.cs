namespace Student_Onboarding_Platform.Models.DTOs.Admin;

public class CourseRegistrationListResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
    public Guid CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public decimal? PaymentAmount { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? Notes { get; set; }
    public DateTime RegisteredAt { get; set; }
}
