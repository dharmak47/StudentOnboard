using Student_Onboarding_Platform.Models.DTOs.Admin;
using Student_Onboarding_Platform.Models.DTOs.Common;

namespace Student_Onboarding_Platform.Services.Interfaces;

public interface IAnalyticsService
{
    /// <summary>
    /// Get monthly analytics data for a date range
    /// </summary>
    Task<ApiResponse<MonthlyAnalyticsDto>> GetMonthlyAnalyticsAsync(
        DateTime startDate,
        DateTime endDate);

    /// <summary>
    /// Get student progress analytics with pagination
    /// </summary>
    Task<ApiResponse<StudentProgressAnalyticsDto>> GetStudentProgressAnalyticsAsync(
        Guid? courseId,
        int page,
        int pageSize);

    /// <summary>
    /// Generate or update monthly analytics for a specific month
    /// </summary>
    Task<ApiResponse<bool>> GenerateMonthlyReportAsync(DateTime yearMonth);

    /// <summary>
    /// Recalculate all analytics data (admin action)
    /// </summary>
    Task<ApiResponse<bool>> UpdateAllAnalyticsAsync();
}
