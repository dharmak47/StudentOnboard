namespace StudentOnboardingApp.Models.Course;

public class CourseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Fees { get; set; }
    public decimal? OfferPrice { get; set; }
    public string Duration { get; set; } = string.Empty;
    public string? Syllabus { get; set; }
    public string? ImageUrl { get; set; }
    public string? Instructor { get; set; }
    public string? Category { get; set; }
    public string? Thumbnail { get; set; }

    public bool HasOfferPrice => OfferPrice.HasValue && OfferPrice.Value > 0 && OfferPrice.Value < Fees;
    public string DisplayThumbnail => string.IsNullOrEmpty(Thumbnail) ? "📘" : Thumbnail;
    public string DisplayInstructor => string.IsNullOrEmpty(Instructor) ? "" : Instructor;
    public string DisplayCategory => string.IsNullOrEmpty(Category) ? "" : Category;
    public bool HasCategory => !string.IsNullOrEmpty(Category);
    public bool HasInstructor => !string.IsNullOrEmpty(Instructor);
}
