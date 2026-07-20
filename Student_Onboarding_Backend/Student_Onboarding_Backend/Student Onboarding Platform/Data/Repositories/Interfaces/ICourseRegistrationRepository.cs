using Student_Onboarding_Platform.Models.Entities;

namespace Student_Onboarding_Platform.Data.Repositories.Interfaces;

public interface ICourseRegistrationRepository
{
    Task<CourseRegistration> CreateAsync(CourseRegistration registration);
    Task<CourseRegistration?> GetByIdAsync(Guid id);
    Task<IEnumerable<CourseRegistration>> GetByUserIdAsync(Guid userId);
    Task<CourseRegistration?> GetByUserAndCourseAsync(Guid userId, Guid courseId);
    Task<IEnumerable<CourseRegistration>> GetAllAsync();
    Task<IEnumerable<CourseRegistration>> GetAllAsync(int offset, int pageSize);
    Task<int> GetAllCountAsync();
    Task<int> GetCountByUserIdAsync(Guid userId);
    Task UpdatePaymentAsync(Guid id, string paymentStatus, decimal? paymentAmount, string? notes);
    Task UpdateAsync(CourseRegistration registration);
    Task<int> GetActiveCountByCourseAsync(Guid courseId);
    Task<IEnumerable<CourseRegistration>> GetIncompleteRegistrationsByCourseAsync(Guid courseId, int offset, int pageSize);
    Task<int> GetIncompleteCountByCourseAsync(Guid courseId);
    Task UpdateCompletionAsync(Guid id, DateTime completedAt, decimal? grade, string? adminNotes, Guid? completedByAdminId);
}
