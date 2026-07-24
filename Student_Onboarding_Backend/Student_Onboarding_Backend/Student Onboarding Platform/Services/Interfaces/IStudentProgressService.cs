using Student_Onboarding_Platform.Models.DTOs.Common;
using Student_Onboarding_Platform.Models.DTOs.Student;

namespace Student_Onboarding_Platform.Services.Interfaces;

public interface IStudentProgressService
{
    /// <summary>
    /// Get detailed progress for a specific course enrollment
    /// </summary>
    Task<ApiResponse<CourseProgressDto>> GetProgressAsync(Guid registrationId);

    /// <summary>
    /// Update progress for a course (admin or system action)
    /// </summary>
    Task<ApiResponse<CourseProgressDto>> UpdateProgressAsync(
        Guid registrationId,
        UpdateProgressRequest request);

    /// <summary>
    /// Get progress summary for a student
    /// </summary>
    Task<ApiResponse<StudentProgressSummaryDto>> GetStudentProgressSummaryAsync(
        Guid studentId);

    /// <summary>
    /// Calculate progress percentage (CompletedModules / TotalModules)
    /// </summary>
    Task<decimal> CalculateProgressPercentageAsync(Guid registrationId);
}
