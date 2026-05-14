using StudentOnboardingApp.Helpers;
using StudentOnboardingApp.Models.Common;
using StudentOnboardingApp.Models.Faq;
using StudentOnboardingApp.Services.Interfaces;

namespace StudentOnboardingApp.Services.Implementations;

public class FaqService : IFaqService
{
    private readonly HttpClient _client;

    public FaqService(IHttpClientFactory httpClientFactory)
    {
        _client = httpClientFactory.CreateClient(Constants.AuthenticatedApiClient);
    }

    public async Task<ApiResponse<List<FaqDto>>> GetFaqsAsync()
    {
        try
        {
            var response = await _client.GetAsync("Student/faqs");
            return await HttpResponseParser.ParseAsync<List<FaqDto>>(response);
        }
        catch
        {
            return new ApiResponse<List<FaqDto>> { Success = false, Message = "Unable to load FAQs." };
        }
    }
}
