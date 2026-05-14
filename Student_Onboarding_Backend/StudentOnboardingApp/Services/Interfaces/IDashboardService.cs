using StudentOnboardingApp.Models.Common;
using StudentOnboardingApp.Models.Dashboard;

namespace StudentOnboardingApp.Services.Interfaces;

public interface IDashboardService
{
    Task<ApiResponse<DashboardDto>> GetDashboardAsync();
}
