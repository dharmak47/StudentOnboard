namespace StudentOnboardingApp.Models.Course;

public class CourseDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Fees { get; set; }
    public string Duration { get; set; } = string.Empty;
    public string Syllabus { get; set; } = string.Empty;
    public List<string> BatchTimings { get; set; } = [];
    public string? ImageUrl { get; set; }
}
