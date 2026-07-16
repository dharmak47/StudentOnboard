using Microsoft.AspNetCore.Http;
using Student_Onboarding_Platform.Models.DTOs.Admin;
using Student_Onboarding_Platform.Models.DTOs.Common;

namespace Student_Onboarding_Platform.Services.Interfaces;

public interface IAdminService
{
    Task<ApiResponse<AdminDashboardResponse>> GetDashboardAsync();
    Task<ApiResponse<PaginatedResponse<StudentListResponse>>> GetStudentsAsync(int page, int pageSize, string? status, string? search);
    Task<ApiResponse<StudentDetailResponse>> GetStudentByIdAsync(Guid studentId);
    Task<ApiResponse<string>> UpdateStudentAsync(Guid studentId, UpdateStudentRequest request);
    Task<ApiResponse<string>> ApproveStudentAsync(Guid studentId, Guid adminId);
    Task<ApiResponse<string>> DenyStudentAsync(Guid studentId, Guid adminId, DenyStudentRequest request);
    Task<ApiResponse<PaginatedResponse<CourseRegistrationListResponse>>> GetCourseRegistrationsAsync(int page, int pageSize);
    Task<ApiResponse<string>> UpdatePaymentAsync(Guid registrationId, UpdatePaymentRequest request);
    Task<ApiResponse<string>> CompleteCourseAsync(Guid registrationId);
    Task<ApiResponse<string>> UploadProfilePhotoAsync(Guid adminId, IFormFile photo);
}
