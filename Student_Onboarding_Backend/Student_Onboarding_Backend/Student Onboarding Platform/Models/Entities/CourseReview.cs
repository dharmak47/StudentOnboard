namespace Student_Onboarding_Platform.Models.Entities;

public class CourseReview
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public Guid UserId { get; set; }
    public int Rating { get; set; }
    public string? Remarks { get; set; }
    public DateTime CreatedAt { get; set; }
}
