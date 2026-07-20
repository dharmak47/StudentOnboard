namespace Student_Onboarding_Platform.Models.DTOs.Student;

public class CourseProgressDto
{
    public Guid RegistrationId { get; set; }
    public Guid CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;

    public DateTime EnrolledDate { get; set; }
    public DateTime? ExpectedCompletionDate { get; set; }
    public int DaysRemaining { get; set; }

    public decimal ProgressPercentage { get; set; }
    public string CurrentModule { get; set; } = string.Empty;

    public int TotalModules { get; set; }
    public int CompletedModules { get; set; }

    public List<ModuleProgressDto> Modules { get; set; } = new();

    public string PaymentStatus { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
}

public class ModuleProgressDto
{
    public int ModuleNumber { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime? CompletedDate { get; set; }
}

public class UpdateProgressRequest
{
    public int CompletedModules { get; set; }
    public string? CurrentModule { get; set; }
    public string? ProgressNotes { get; set; }
}

public class StudentProgressSummaryDto
{
    public Guid StudentId { get; set; }
    public int EnrolledCourses { get; set; }
    public int ActiveCourses { get; set; }
    public int CompletedCourses { get; set; }
    public decimal AverageProgress { get; set; }
    public int AtRiskCourses { get; set; }
}
