using System.Globalization;
using Student_Onboarding_Platform.Data.Repositories.Interfaces;
using Student_Onboarding_Platform.Models.DTOs.Admin;
using Student_Onboarding_Platform.Models.DTOs.Common;
using Student_Onboarding_Platform.Models.Entities;
using Student_Onboarding_Platform.Services.Interfaces;

namespace Student_Onboarding_Platform.Services.Implementations;

public class AnalyticsService : IAnalyticsService
{
    private readonly IMonthlyAnalyticsRepository _analyticsRepository;
    private readonly ICourseRegistrationRepository _registrationRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AnalyticsService> _logger;

    public AnalyticsService(
        IMonthlyAnalyticsRepository analyticsRepository,
        ICourseRegistrationRepository registrationRepository,
        IUserRepository userRepository,
        ILogger<AnalyticsService> logger)
    {
        _analyticsRepository = analyticsRepository;
        _registrationRepository = registrationRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<ApiResponse<MonthlyAnalyticsDto>> GetMonthlyAnalyticsAsync(
        DateTime startDate,
        DateTime endDate)
    {
        try
        {
            // Normalize dates to first day of month
            var start = new DateTime(startDate.Year, startDate.Month, 1);
            var end = new DateTime(endDate.Year, endDate.Month, 1);

            // Generate missing analytics for months in the range
            var currentMonth = start;
            while (currentMonth <= end)
            {
                var existing = await _analyticsRepository.GetByMonthAsync(currentMonth);
                if (existing == null)
                {
                    // Generate analytics for this month
                    await GenerateMonthlyReportAsync(currentMonth);
                }
                currentMonth = currentMonth.AddMonths(1);
            }

            var analyticsRecords = await _analyticsRepository.GetRangeAsync(start, end);
            var records = analyticsRecords.ToList();

            var months = new List<MonthlyMetricsDto>();

            foreach (var record in records)
            {
                months.Add(new MonthlyMetricsDto
                {
                    YearMonth = record.YearMonth,
                    Month = record.YearMonth.ToString("MMMM yyyy", CultureInfo.InvariantCulture),
                    Metrics = new MetricsDto
                    {
                        NewEnrollments = record.NewEnrollments,
                        TotalEnrollments = record.TotalEnrollments,
                        CompletedCourses = record.CompletedCourses,
                        PendingCompletions = record.PendingCompletions,
                        TotalRevenue = record.TotalRevenueCollected,
                        PaymentsCompleted = record.PaymentsCompleted,
                        PaymentsPending = record.PaymentsPending,
                        ActiveStudents = record.ActiveStudents,
                        ApprovedStudents = record.ApprovedStudents,
                        PendingApprovals = record.PendingApprovals,
                        AverageCompletionPercentage = record.AverageCompletionPercentage,
                        CoursePassRate = record.CoursePassRate
                    }
                });
            }

            // Calculate summary
            var summary = new AnalyticsSummaryDto
            {
                TotalNewEnrollments = records.Sum(r => r.NewEnrollments),
                TotalCompletions = records.Sum(r => r.CompletedCourses),
                TotalRevenue = records.Sum(r => r.TotalRevenueCollected),
                AverageMonthlyGrowth = records.Count > 1
                    ? (decimal)records.Average(r => r.NewEnrollments)
                    : 0
            };

            var dto = new MonthlyAnalyticsDto
            {
                Months = months,
                Summary = summary
            };

            _logger.LogInformation("Retrieved monthly analytics from {StartDate} to {EndDate}",
                start, end);

            return ApiResponse<MonthlyAnalyticsDto>.Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving monthly analytics");
            return ApiResponse<MonthlyAnalyticsDto>.Fail($"Error retrieving analytics: {ex.Message}");
        }
    }

    public async Task<ApiResponse<StudentProgressAnalyticsDto>> GetStudentProgressAnalyticsAsync(
        Guid? courseId,
        int page,
        int pageSize)
    {
        try
        {
            var registrations = await _registrationRepository.GetAllAsync();
            var registrationList = registrations.ToList();

            // Filter by course if specified
            if (courseId.HasValue)
            {
                registrationList = registrationList
                    .Where(r => r.CourseId == courseId.Value)
                    .ToList();
            }

            // Group by student
            var studentProgress = new Dictionary<Guid, StudentProgressRecordDto>();

            foreach (var reg in registrationList)
            {
                if (!studentProgress.ContainsKey(reg.UserId))
                {
                    var user = await _userRepository.GetByIdAsync(reg.UserId);
                    studentProgress[reg.UserId] = new StudentProgressRecordDto
                    {
                        StudentId = reg.UserId,
                        StudentName = $"{user?.FirstName} {user?.LastName}".Trim(),
                        Email = user?.Email ?? string.Empty,
                        Courses = new List<StudentCourseProgressDto>()
                    };
                }

                studentProgress[reg.UserId].Courses.Add(new StudentCourseProgressDto
                {
                    RegistrationId = reg.Id,
                    CourseName = $"Course {reg.CourseId}",
                    ProgressPercentage = reg.ProgressPercentage,
                    EnrolledDate = reg.CreatedAt,
                    ExpectedCompletionDate = reg.ExpectedCompletionDate,
                    IsAtRisk = reg.ProgressPercentage < 30 && !reg.IsCompleted,
                    PaymentStatus = reg.PaymentStatus
                });
            }

            // Calculate overall progress for each student
            foreach (var student in studentProgress.Values)
            {
                student.OverallProgress = student.Courses.Any()
                    ? student.Courses.Average(c => c.ProgressPercentage)
                    : 0;
                student.AtRiskCourses = student.Courses.Count(c => c.IsAtRisk);
            }

            // Apply pagination
            var totalCount = studentProgress.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            var students = studentProgress.Values
                .OrderByDescending(s => s.OverallProgress)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Calculate summary
            var allStudents = studentProgress.Values.ToList();
            var summary = new StudentProgressSummaryDto
            {
                OnTrackStudents = allStudents.Count(s => s.OverallProgress >= 70),
                AtRiskStudents = allStudents.Count(s => s.OverallProgress >= 30 && s.OverallProgress < 70),
                BehindScheduleStudents = allStudents.Count(s => s.OverallProgress < 30),
                AverageProgress = allStudents.Any() ? allStudents.Average(s => s.OverallProgress) : 0
            };

            var dto = new StudentProgressAnalyticsDto
            {
                Students = students,
                Pagination = new PaginationDto
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages
                },
                Summary = summary
            };

            _logger.LogInformation("Retrieved student progress analytics, Total students: {Count}, Page: {Page}",
                totalCount, page);

            return ApiResponse<StudentProgressAnalyticsDto>.Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving student progress analytics");
            return ApiResponse<StudentProgressAnalyticsDto>.Fail($"Error retrieving student progress: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> GenerateMonthlyReportAsync(DateTime yearMonth)
    {
        try
        {
            var monthStart = new DateTime(yearMonth.Year, yearMonth.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var registrations = await _registrationRepository.GetAllAsync();
            var regList = registrations.ToList();

            // Calculate metrics for the month
            var newEnrollments = regList.Count(r =>
                r.CreatedAt.Date >= monthStart.Date && r.CreatedAt.Date <= monthEnd.Date);

            var monthlyRegistrations = regList.Where(r => r.CreatedAt <= monthEnd).ToList();
            var completed = monthlyRegistrations.Count(r => r.IsCompleted);
            var totalRevenue = monthlyRegistrations
                .Where(r => r.PaymentStatus == "Completed")
                .Sum(r => r.PaymentAmount ?? 0);

            var users = await _userRepository.GetAllAsync();
            var userList = users.ToList();

            var analytics = new MonthlyAnalytics
            {
                YearMonth = monthStart,
                NewEnrollments = newEnrollments,
                TotalEnrollments = monthlyRegistrations.Count,
                CompletedCourses = completed,
                PendingCompletions = monthlyRegistrations.Count(r => !r.IsCompleted),
                TotalRevenueCollected = totalRevenue,
                PaymentsCompleted = monthlyRegistrations.Count(r => r.PaymentStatus == "Completed"),
                PaymentsPending = monthlyRegistrations.Count(r => r.PaymentStatus == "Pending"),
                ActiveStudents = monthlyRegistrations.Select(r => r.UserId).Distinct().Count(),
                ApprovedStudents = userList.Count(u => u.ApprovalStatus == "Approved"),
                PendingApprovals = userList.Count(u => u.ApprovalStatus == "Pending"),
                AverageCompletionPercentage = monthlyRegistrations.Any()
                    ? (decimal)monthlyRegistrations.Average(r => r.ProgressPercentage)
                    : 0,
                CoursePassRate = completed > 0
                    ? Math.Round((decimal)completed / monthlyRegistrations.Count * 100, 2)
                    : 0
            };

            var existing = await _analyticsRepository.GetByMonthAsync(monthStart);
            if (existing != null)
            {
                analytics.Id = existing.Id;
                analytics.CreatedAt = existing.CreatedAt;
                await _analyticsRepository.UpdateAsync(analytics);
            }
            else
            {
                await _analyticsRepository.CreateAsync(analytics);
            }

            _logger.LogInformation("Generated monthly report for {YearMonth}", monthStart);
            return ApiResponse<bool>.Ok(true, "Monthly report generated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating monthly report for {YearMonth}", yearMonth);
            return ApiResponse<bool>.Fail($"Error generating report: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> UpdateAllAnalyticsAsync()
    {
        try
        {
            // Generate analytics for last 12 months
            var startDate = DateTime.UtcNow.AddMonths(-12);
            for (int i = 0; i <= 12; i++)
            {
                var monthDate = new DateTime(startDate.Year, startDate.Month, 1).AddMonths(i);
                await GenerateMonthlyReportAsync(monthDate);
            }

            _logger.LogInformation("Updated all analytics for the last 12 months");
            return ApiResponse<bool>.Ok(true, "All analytics updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating all analytics");
            return ApiResponse<bool>.Fail($"Error updating analytics: {ex.Message}");
        }
    }
}
