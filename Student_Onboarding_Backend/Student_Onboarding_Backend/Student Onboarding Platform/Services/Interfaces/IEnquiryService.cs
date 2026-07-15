using Student_Onboarding_Platform.Models.DTOs;
using Student_Onboarding_Platform.Models.DTOs.Common;
using Student_Onboarding_Platform.Models.Entities;

namespace Student_Onboarding_Platform.Services.Interfaces;

public interface IEnquiryService
{
    Task<ApiResponse<EnquiryDto>> CreateEnquiryAsync(EnquiryRequestDto requestDto);
    Task<ApiResponse<IEnumerable<EnquiryDto>>> GetAllEnquiriesAsync();
    Task<ApiResponse<bool>> ResolveEnquiryAsync(Guid id);
}
