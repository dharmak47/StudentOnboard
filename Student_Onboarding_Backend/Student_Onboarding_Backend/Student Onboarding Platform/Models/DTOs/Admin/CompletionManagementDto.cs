namespace Student_Onboarding_Platform.Models.DTOs.Admin;

/// <summary>
/// Request DTO for updating course completion status
/// </summary>
public class UpdateCompletionRequest
{
    /// <summary>
    /// The course registration ID to mark complete
    /// </summary>
    public Guid RegistrationId { get; set; }

    /// <summary>
    /// The grade/score for the completed course (0-100)
    /// </summary>
    public decimal? Grade { get; set; }

    /// <summary>
    /// Admin notes about the completion
    /// </summary>
    public string? AdminNotes { get; set; }

    /// <summary>
    /// Optional completion date (defaults to today if not provided)
    /// </summary>
    public DateTime? CompletionDate { get; set; }
}

/// <summary>
/// Response DTO for course completion details
/// </summary>
public class CompletionResponseDto
{
    public Guid RegistrationId { get; set; }
    public Guid UserId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
    public Guid CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public decimal? Grade { get; set; }
    public string? AdminNotes { get; set; }
    public Guid? CompletedByAdminId { get; set; }
    public string? AdminName { get; set; }
    public decimal ProgressPercentage { get; set; }
    public DateTime EnrolledDate { get; set; }
    public DateTime? ExpectedCompletionDate { get; set; }
}

/// <summary>
/// DTO for listing incomplete course registrations
/// </summary>
public class IncompleteRegistrationDto
{
    public Guid RegistrationId { get; set; }
    public Guid UserId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
    public Guid CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public decimal ProgressPercentage { get; set; }
    public DateTime EnrolledDate { get; set; }
    public DateTime? ExpectedCompletionDate { get; set; }
    public bool IsAtRisk { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public List<string> CompletedModules { get; set; } = new();
}

/// <summary>
/// Request DTO for batch completion operations
/// </summary>
public class BulkCompletionRequest
{
    public List<Guid> RegistrationIds { get; set; } = new();
    public decimal? Grade { get; set; }
    public string? AdminNotes { get; set; }
    public DateTime? CompletionDate { get; set; }
}
