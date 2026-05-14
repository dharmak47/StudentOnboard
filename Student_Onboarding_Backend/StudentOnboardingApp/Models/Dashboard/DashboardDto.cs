namespace StudentOnboardingApp.Models.Dashboard;

public class DashboardDto
{
    public string ApprovalStatus { get; set; } = string.Empty;
    public int RegisteredCoursesCount { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // Active course info
    public string? CourseName { get; set; }
    public string? CourseDuration { get; set; }
    public string? CourseStatus { get; set; } // "Ongoing", "Completed", "Pending Payment"
    public string? PaymentStatus { get; set; }
    public decimal? AmountDue { get; set; }
    public DateTime? EnrolledDate { get; set; }
    public string? BatchTiming { get; set; }

    // Previously completed courses
    public List<CompletedCourseDto> CompletedCourses { get; set; } = [];
}

public class CompletedCourseDto
{
    public string CourseName { get; set; } = string.Empty;
    public string? Duration { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime EnrolledAt { get; set; }
}
