namespace Student_Onboarding_Platform.Models.Entities;

public class MonthlyAnalytics
{
    public Guid Id { get; set; }

    // Period tracking (first day of month)
    public DateTime YearMonth { get; set; }

    // Enrollment metrics
    public int NewEnrollments { get; set; }
    public int TotalEnrollments { get; set; }

    // Completion metrics
    public int CompletedCourses { get; set; }
    public int PendingCompletions { get; set; }

    // Payment metrics
    public decimal TotalRevenueCollected { get; set; }
    public int PaymentsCompleted { get; set; }
    public int PaymentsPending { get; set; }

    // User metrics
    public int ActiveStudents { get; set; }
    public int ApprovedStudents { get; set; }
    public int PendingApprovals { get; set; }

    // Performance metrics
    public decimal AverageCompletionPercentage { get; set; }
    public decimal CoursePassRate { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
}
