using Student_Onboarding_Platform.Models.Entities;

namespace Student_Onboarding_Platform.Data.Repositories.Interfaces;

public interface ICourseRepository
{
    Task<Course> CreateAsync(Course course);
    Task<Course?> GetByIdAsync(Guid id);
    Task<IEnumerable<Course>> GetActiveCoursesAsync();
    Task UpdateAsync(Course course);
    Task SoftDeleteAsync(Guid id);
}
