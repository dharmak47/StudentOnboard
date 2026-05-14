using Student_Onboarding_Platform.Models.Entities;

namespace Student_Onboarding_Platform.Data.Repositories.Interfaces;

public interface ICourseReviewRepository
{
    Task<CourseReview> CreateAsync(CourseReview review);
    Task<CourseReview?> GetByUserAndCourseAsync(Guid userId, Guid courseId);
    Task<IEnumerable<CourseReview>> GetByCourseIdAsync(Guid courseId);
    Task<double?> GetAverageRatingAsync(Guid courseId);
    Task<int> GetReviewCountAsync(Guid courseId);
}
