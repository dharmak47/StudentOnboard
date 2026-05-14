using Student_Onboarding_Platform.Models.DTOs.Admin;
using Student_Onboarding_Platform.Models.DTOs.Common;

namespace Student_Onboarding_Platform.Services.Interfaces;

public interface IFaqService
{
    Task<ApiResponse<List<FaqResponse>>> GetAllFaqsAsync();
    Task<ApiResponse<List<FaqResponse>>> GetActiveFaqsAsync();
    Task<ApiResponse<FaqResponse>> CreateFaqAsync(CreateFaqRequest request, Guid? createdBy);
    Task<ApiResponse<FaqResponse>> UpdateFaqAsync(Guid id, UpdateFaqRequest request);
    Task<ApiResponse<string>> DeleteFaqAsync(Guid id);
}
