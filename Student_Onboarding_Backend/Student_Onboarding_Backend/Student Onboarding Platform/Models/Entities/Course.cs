namespace Student_Onboarding_Platform.Models.Entities;

public class Course
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Fees { get; set; }
    public decimal? OfferPrice { get; set; }
    public string? Syllabus { get; set; }
    public string? Duration { get; set; }
    public string? Instructor { get; set; }
    public string? Category { get; set; }
    public string? Thumbnail { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
