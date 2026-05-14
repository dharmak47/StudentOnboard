namespace Student_Onboarding_Platform.Models.DTOs.Course;

public class SubmitReviewRequest
{
    public int Rating { get; set; }
    public string? Remarks { get; set; }
}

public class CourseReviewResponse
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public Guid UserId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Remarks { get; set; }
    public DateTime CreatedAt { get; set; }
}
