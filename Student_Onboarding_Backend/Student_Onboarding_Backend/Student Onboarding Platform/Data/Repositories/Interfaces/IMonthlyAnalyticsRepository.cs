using Student_Onboarding_Platform.Models.Entities;

namespace Student_Onboarding_Platform.Data.Repositories.Interfaces;

public interface IMonthlyAnalyticsRepository
{
    Task<MonthlyAnalytics?> GetByMonthAsync(DateTime yearMonth);
    Task<IEnumerable<MonthlyAnalytics>> GetRangeAsync(DateTime startDate, DateTime endDate);
    Task<MonthlyAnalytics> CreateAsync(MonthlyAnalytics analytics);
    Task<MonthlyAnalytics> UpdateAsync(MonthlyAnalytics analytics);
    Task<bool> ExistsAsync(DateTime yearMonth);
}
