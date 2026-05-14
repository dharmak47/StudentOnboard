using Student_Onboarding_Platform.Data.Repositories.Interfaces;
using Student_Onboarding_Platform.Models.DTOs.Admin;
using Student_Onboarding_Platform.Models.DTOs.Common;
using Student_Onboarding_Platform.Models.Entities;
using Student_Onboarding_Platform.Services.Interfaces;

namespace Student_Onboarding_Platform.Services.Implementations;

public class FaqService : IFaqService
{
    private readonly IFaqRepository _faqRepository;
    private readonly ILogger<FaqService> _logger;

    public FaqService(IFaqRepository faqRepository, ILogger<FaqService> logger)
    {
        _faqRepository = faqRepository;
        _logger = logger;
    }

    public async Task<ApiResponse<List<FaqResponse>>> GetAllFaqsAsync()
    {
        var faqs = await _faqRepository.GetAllAsync();
        var result = faqs.Select(MapToResponse).ToList();
        return ApiResponse<List<FaqResponse>>.Ok(result);
    }

    public async Task<ApiResponse<List<FaqResponse>>> GetActiveFaqsAsync()
    {
        var faqs = await _faqRepository.GetActiveAsync();
        var result = faqs.Select(MapToResponse).ToList();
        return ApiResponse<List<FaqResponse>>.Ok(result);
    }

    public async Task<ApiResponse<FaqResponse>> CreateFaqAsync(CreateFaqRequest request, Guid? createdBy)
    {
        _logger.LogInformation("Creating FAQ by admin {AdminId}", createdBy);

        var faq = new Faq
        {
            Question = request.Question,
            Answer = request.Answer,
            SortOrder = request.SortOrder,
            IsActive = true,
            IsDeleted = false,
            CreatedBy = createdBy == Guid.Empty ? null : createdBy
        };

        await _faqRepository.CreateAsync(faq);
        _logger.LogInformation("FAQ created: {FaqId}", faq.Id);

        return ApiResponse<FaqResponse>.Ok(MapToResponse(faq), "FAQ created successfully.");
    }

    public async Task<ApiResponse<FaqResponse>> UpdateFaqAsync(Guid id, UpdateFaqRequest request)
    {
        var faq = await _faqRepository.GetByIdAsync(id);
        if (faq == null)
            return ApiResponse<FaqResponse>.Fail("FAQ not found.");

        faq.Question = request.Question;
        faq.Answer = request.Answer;
        faq.SortOrder = request.SortOrder;
        faq.IsActive = request.IsActive;

        await _faqRepository.UpdateAsync(faq);
        _logger.LogInformation("FAQ updated: {FaqId}", id);

        return ApiResponse<FaqResponse>.Ok(MapToResponse(faq), "FAQ updated successfully.");
    }

    public async Task<ApiResponse<string>> DeleteFaqAsync(Guid id)
    {
        var faq = await _faqRepository.GetByIdAsync(id);
        if (faq == null)
            return ApiResponse<string>.Fail("FAQ not found.");

        await _faqRepository.SoftDeleteAsync(id);
        _logger.LogInformation("FAQ deleted: {FaqId}", id);

        return ApiResponse<string>.Ok("Deleted", "FAQ deleted successfully.");
    }

    private static FaqResponse MapToResponse(Faq faq)
    {
        return new FaqResponse
        {
            Id = faq.Id,
            Question = faq.Question,
            Answer = faq.Answer,
            SortOrder = faq.SortOrder,
            IsActive = faq.IsActive,
            CreatedAt = faq.CreatedAt,
            UpdatedAt = faq.UpdatedAt
        };
    }
}
