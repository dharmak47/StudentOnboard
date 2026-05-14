using StudentOnboardingApp.Helpers;
using StudentOnboardingApp.Models.Common;
using StudentOnboardingApp.Models.Dashboard;
using StudentOnboardingApp.Services.Interfaces;

namespace StudentOnboardingApp.Services.Implementations;

public class DashboardService : IDashboardService
{
    private readonly HttpClient _client;

    public DashboardService(IHttpClientFactory httpClientFactory)
    {
        _client = httpClientFactory.CreateClient(Constants.AuthenticatedApiClient);
    }

    public async Task<ApiResponse<DashboardDto>> GetDashboardAsync()
    {
        try
        {
            var response = await _client.GetAsync("student/dashboard");
            return await HttpResponseParser.ParseAsync<DashboardDto>(response);
        }
        catch (Exception ex)
        {
            return new ApiResponse<DashboardDto> { Success = false, Message = "Unable to load dashboard. Please try again." };
        }
    }
}
