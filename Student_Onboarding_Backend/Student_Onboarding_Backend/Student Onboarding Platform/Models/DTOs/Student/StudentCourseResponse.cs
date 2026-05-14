namespace Student_Onboarding_Platform.Models.DTOs.Student;

public class StudentCourseResponse
{
    public Guid RegistrationId { get; set; }
    public Guid CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string? CourseDescription { get; set; }
    public decimal CourseFees { get; set; }
    public decimal? CourseOfferPrice { get; set; }
    public string? Duration { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public decimal? PaymentAmount { get; set; }
    public DateTime RegisteredAt { get; set; }
}
