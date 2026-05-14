using Student_Onboarding_Platform.Models.DTOs.Admin;
using Student_Onboarding_Platform.Models.DTOs.Common;
using Student_Onboarding_Platform.Models.DTOs.Course;

namespace Student_Onboarding_Platform.Services.Interfaces;

public interface ICourseService
{
    Task<ApiResponse<List<CourseListResponse>>> GetActiveCoursesAsync();
    Task<ApiResponse<CourseDetailResponse>> GetCourseByIdAsync(Guid courseId);
    Task<ApiResponse<CourseDetailResponse>> CreateCourseAsync(CreateCourseRequest request, Guid createdBy);
    Task<ApiResponse<CourseDetailResponse>> UpdateCourseAsync(Guid courseId, UpdateCourseRequest request);
    Task<ApiResponse<string>> DeleteCourseAsync(Guid courseId);
}
