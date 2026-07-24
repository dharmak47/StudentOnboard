using Student_Onboarding_Platform.Models.Entities;

namespace Student_Onboarding_Platform.Data.Repositories.Interfaces;

public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(Guid id);
    Task<Invoice?> GetByRegistrationIdAsync(Guid registrationId);
    Task<IEnumerable<Invoice>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Invoice>> GetAllAsync(int offset, int pageSize);
    Task<int> GetAllCountAsync();
    Task<IEnumerable<InvoiceItem>> GetItemsAsync(Guid invoiceId);
    Task<int> GetMaxSequenceForYearAsync(int year);
    Task CreateAsync(Invoice invoice, IEnumerable<InvoiceItem> items);
    Task UpdateAsync(Invoice invoice, IEnumerable<InvoiceItem> items);
}
