using Dapper;
using Student_Onboarding_Platform.Data.Repositories.Interfaces;
using Student_Onboarding_Platform.Models.Entities;

namespace Student_Onboarding_Platform.Data.Repositories.Implementations;

public class MonthlyAnalyticsRepository : IMonthlyAnalyticsRepository
{
    private readonly DbConnectionFactory _db;

    public MonthlyAnalyticsRepository(DbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<MonthlyAnalytics?> GetByMonthAsync(DateTime yearMonth)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<MonthlyAnalytics>(
            @"SELECT * FROM MonthlyAnalytics
              WHERE YearMonth = @YearMonth",
            new { YearMonth = yearMonth.Date });
    }

    public async Task<IEnumerable<MonthlyAnalytics>> GetRangeAsync(DateTime startDate, DateTime endDate)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<MonthlyAnalytics>(
            @"SELECT * FROM MonthlyAnalytics
              WHERE YearMonth BETWEEN @StartDate AND @EndDate
              ORDER BY YearMonth ASC",
            new { StartDate = startDate.Date, EndDate = endDate.Date });
    }

    public async Task<MonthlyAnalytics> CreateAsync(MonthlyAnalytics analytics)
    {
        analytics.Id = Guid.NewGuid();
        analytics.CreatedAt = DateTime.UtcNow;
        analytics.UpdatedAt = DateTime.UtcNow;

        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(@"
            INSERT INTO MonthlyAnalytics (
                Id, YearMonth, NewEnrollments, TotalEnrollments,
                CompletedCourses, PendingCompletions, TotalRevenueCollected,
                PaymentsCompleted, PaymentsPending, ActiveStudents,
                ApprovedStudents, PendingApprovals,
                AverageCompletionPercentage, CoursePassRate,
                CreatedAt, UpdatedAt, UpdatedBy)
            VALUES (
                @Id, @YearMonth, @NewEnrollments, @TotalEnrollments,
                @CompletedCourses, @PendingCompletions, @TotalRevenueCollected,
                @PaymentsCompleted, @PaymentsPending, @ActiveStudents,
                @ApprovedStudents, @PendingApprovals,
                @AverageCompletionPercentage, @CoursePassRate,
                @CreatedAt, @UpdatedAt, @UpdatedBy)",
            analytics);

        return analytics;
    }

    public async Task<MonthlyAnalytics> UpdateAsync(MonthlyAnalytics analytics)
    {
        analytics.UpdatedAt = DateTime.UtcNow;

        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(@"
            UPDATE MonthlyAnalytics SET
                NewEnrollments = @NewEnrollments,
                TotalEnrollments = @TotalEnrollments,
                CompletedCourses = @CompletedCourses,
                PendingCompletions = @PendingCompletions,
                TotalRevenueCollected = @TotalRevenueCollected,
                PaymentsCompleted = @PaymentsCompleted,
                PaymentsPending = @PaymentsPending,
                ActiveStudents = @ActiveStudents,
                ApprovedStudents = @ApprovedStudents,
                PendingApprovals = @PendingApprovals,
                AverageCompletionPercentage = @AverageCompletionPercentage,
                CoursePassRate = @CoursePassRate,
                UpdatedAt = @UpdatedAt,
                UpdatedBy = @UpdatedBy
            WHERE YearMonth = @YearMonth",
            analytics);

        return analytics;
    }

    public async Task<bool> ExistsAsync(DateTime yearMonth)
    {
        using var conn = _db.CreateConnection();
        var result = await conn.QueryFirstOrDefaultAsync<int>(
            @"SELECT COUNT(1) FROM MonthlyAnalytics
              WHERE YearMonth = @YearMonth",
            new { YearMonth = yearMonth.Date });
        return result > 0;
    }
}
