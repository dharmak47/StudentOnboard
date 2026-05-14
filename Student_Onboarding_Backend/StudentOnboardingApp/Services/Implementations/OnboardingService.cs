using System.Net.Http.Json;
using StudentOnboardingApp.Helpers;
using StudentOnboardingApp.Models.Common;
using StudentOnboardingApp.Models.Onboarding;
using StudentOnboardingApp.Services.Interfaces;

namespace StudentOnboardingApp.Services.Implementations;

public class OnboardingService : IOnboardingService
{
    private readonly HttpClient _client;

    public OnboardingService(IHttpClientFactory httpClientFactory)
    {
        _client = httpClientFactory.CreateClient(Constants.AuthenticatedApiClient);
    }

    public async Task<ApiResponse<ApprovalStatusResponse>> GetApprovalStatusAsync(string email)
    {
        try
        {
            var request = new { Email = email };
            var response = await _client.PostAsJsonAsync("auth/check-approval-status", request);
            return await HttpResponseParser.ParseAsync<ApprovalStatusResponse>(response);
        }
        catch
        {
            return new ApiResponse<ApprovalStatusResponse> { Success = false, Message = "Unable to check approval status." };
        }
    }

    public async Task<ApiResponse<List<OnboardingInstructionDto>>> GetInstructionsAsync()
    {
        try
        {
            var response = await _client.GetAsync("onboarding/instructions");
            return await HttpResponseParser.ParseAsync<List<OnboardingInstructionDto>>(response);
        }
        catch
        {
            return new ApiResponse<List<OnboardingInstructionDto>> { Success = false, Message = "Unable to load instructions." };
        }
    }

    public async Task<ApiResponse<string>> AcceptOnboardingAsync()
    {
        try
        {
            var response = await _client.PostAsync("onboarding/accept", null);
            return await HttpResponseParser.ParseAsync<string>(response);
        }
        catch
        {
            return new ApiResponse<string> { Success = false, Message = "Failed to accept onboarding." };
        }
    }
}
