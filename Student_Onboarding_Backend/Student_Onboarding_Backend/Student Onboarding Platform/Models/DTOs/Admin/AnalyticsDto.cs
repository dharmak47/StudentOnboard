namespace Student_Onboarding_Platform.Models.DTOs.Admin;

public class MonthlyAnalyticsDto
{
    public List<MonthlyMetricsDto> Months { get; set; } = new();
    public AnalyticsSummaryDto? Summary { get; set; }
}

public class MonthlyMetricsDto
{
    public DateTime YearMonth { get; set; }
    public string Month { get; set; } = string.Empty;
    public MetricsDto? Metrics { get; set; }
}

public class MetricsDto
{
    public int NewEnrollments { get; set; }
    public int TotalEnrollments { get; set; }
    public int CompletedCourses { get; set; }
    public int PendingCompletions { get; set; }
    public decimal TotalRevenue { get; set; }
    public int PaymentsCompleted { get; set; }
    public int PaymentsPending { get; set; }
    public int ActiveStudents { get; set; }
    public int ApprovedStudents { get; set; }
    public int PendingApprovals { get; set; }
    public decimal AverageCompletionPercentage { get; set; }
    public decimal CoursePassRate { get; set; }
}

public class AnalyticsSummaryDto
{
    public int TotalNewEnrollments { get; set; }
    public int TotalCompletions { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageMonthlyGrowth { get; set; }
}

public class StudentProgressAnalyticsDto
{
    public List<StudentProgressRecordDto> Students { get; set; } = new();
    public PaginationDto? Pagination { get; set; }
    public StudentProgressSummaryDto? Summary { get; set; }
}

public class StudentProgressRecordDto
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<StudentCourseProgressDto> Courses { get; set; } = new();
    public decimal OverallProgress { get; set; }
    public int AtRiskCourses { get; set; }
}

public class StudentCourseProgressDto
{
    public Guid RegistrationId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public decimal ProgressPercentage { get; set; }
    public DateTime EnrolledDate { get; set; }
    public DateTime? ExpectedCompletionDate { get; set; }
    public bool IsAtRisk { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
}

public class StudentProgressSummaryDto
{
    public int OnTrackStudents { get; set; }
    public int AtRiskStudents { get; set; }
    public int BehindScheduleStudents { get; set; }
    public decimal AverageProgress { get; set; }
}

public class PaginationDto
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}
