using StudentOnboardingApp.Models.Common;
using StudentOnboardingApp.Models.Course;

namespace StudentOnboardingApp.Services.Interfaces;

public interface ICourseService
{
    Task<ApiResponse<List<CourseDto>>> GetCoursesAsync();
    Task<ApiResponse<CourseDetailDto>> GetCourseDetailAsync(Guid courseId);
    Task<ApiResponse<string>> ApplyForCourseAsync(CourseApplicationRequest request);
    Task<ApiResponse<CourseReviewsResponse>> GetCourseReviewsAsync(Guid courseId);
    Task<ApiResponse<string>> SubmitReviewAsync(Guid courseId, SubmitReviewRequest request);
}
