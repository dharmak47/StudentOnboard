namespace Student_Onboarding_Platform.Models.Entities;

public class CourseRegistration
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }
    public string PaymentStatus { get; set; } = "Pending";
    public decimal? PaymentAmount { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ExpectedCompletionDate { get; set; }
    public string? CurrentModule { get; set; }
    public int TotalModules { get; set; }
    public int CompletedModules { get; set; }
    public decimal ProgressPercentage { get; set; }
    public DateTime? LastProgressUpdated { get; set; }
    public decimal? Grade { get; set; }
    public string? AdminNotes { get; set; }
    public Guid? CompletedByAdminId { get; set; }
}
