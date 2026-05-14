using Student_Onboarding_Platform.Models.Entities;

namespace Student_Onboarding_Platform.Data.Repositories.Interfaces;

public interface IFaqRepository
{
    Task<Faq> CreateAsync(Faq faq);
    Task<Faq?> GetByIdAsync(Guid id);
    Task<IEnumerable<Faq>> GetAllAsync();
    Task<IEnumerable<Faq>> GetActiveAsync();
    Task UpdateAsync(Faq faq);
    Task SoftDeleteAsync(Guid id);
}
