using Microsoft.AspNetCore.Http;
using Student_Onboarding_Platform.Models.DTOs.Common;
using Student_Onboarding_Platform.Models.DTOs.Student;

namespace Student_Onboarding_Platform.Services.Interfaces;

public interface IStudentService
{
    Task<ApiResponse<StudentProfileResponse>> GetProfileAsync(Guid userId);
    Task<ApiResponse<StudentProfileResponse>> UpdateProfileAsync(Guid userId, UpdateProfileRequest request);
    Task<ApiResponse<StudentProfileResponse>> UploadProfilePhotoAsync(Guid userId, IFormFile photo);
    Task<ApiResponse<StudentDashboardResponse>> GetDashboardAsync(Guid userId);
    Task<ApiResponse<List<StudentCourseResponse>>> GetRegisteredCoursesAsync(Guid userId);
    Task<ApiResponse<string>> RegisterForCourseAsync(Guid userId, CourseRegistrationRequest request);
}
