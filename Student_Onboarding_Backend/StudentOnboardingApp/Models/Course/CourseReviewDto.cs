namespace StudentOnboardingApp.Models.Course;

public class CourseReviewDto
{
    public Guid Id { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Remarks { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CourseReviewsResponse
{
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public bool CanReview { get; set; }
    public bool HasReviewed { get; set; }
    public List<CourseReviewDto> Reviews { get; set; } = [];
}

public class SubmitReviewRequest
{
    public int Rating { get; set; }
    public string? Remarks { get; set; }
}
