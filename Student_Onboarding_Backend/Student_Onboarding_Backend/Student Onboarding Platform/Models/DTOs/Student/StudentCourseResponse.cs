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

    // Progress tracking
    public string Status { get; set; } = string.Empty; // "Pending", "Active", "Completed"
    public decimal ProgressPercentage { get; set; } = 0;
    public int CompletedModules { get; set; } = 0;
    public int TotalModules { get; set; } = 0;
    public DateTime? ExpectedCompletionDate { get; set; }
    public int DaysRemaining { get; set; } = 0;
    public bool IsCompleted { get; set; } = false;
}
