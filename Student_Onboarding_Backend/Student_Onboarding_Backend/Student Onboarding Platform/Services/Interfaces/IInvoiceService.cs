using Student_Onboarding_Platform.Models.DTOs.Common;
using Student_Onboarding_Platform.Models.DTOs.Invoice;

namespace Student_Onboarding_Platform.Services.Interfaces;

public interface IInvoiceService
{
    /// <summary>
    /// Returns the invoice for a registration, creating it on first access (idempotent).
    /// When <paramref name="isAdmin"/> is false, the registration must belong to
    /// <paramref name="requestingUserId"/> and have a non-Pending payment status.
    /// </summary>
    Task<ApiResponse<InvoiceDto>> GetOrCreateForRegistrationAsync(Guid registrationId, Guid? requestingUserId, bool isAdmin);

    Task<ApiResponse<IEnumerable<InvoiceListItemDto>>> GetForStudentAsync(Guid userId);
    Task<ApiResponse<InvoiceDto>> GetByIdForStudentAsync(Guid invoiceId, Guid userId);

    Task<ApiResponse<PaginatedResponse<InvoiceListItemDto>>> GetAllAsync(int page, int pageSize);
    Task<ApiResponse<InvoiceDto>> GetByIdAsync(Guid invoiceId);
    Task<ApiResponse<InvoiceDto>> UpdateAsync(Guid invoiceId, UpdateInvoiceRequest request);

    Task<ApiResponse<OrganizationSettingsDto>> GetOrganizationSettingsAsync();
    Task<ApiResponse<OrganizationSettingsDto>> UpdateOrganizationSettingsAsync(UpdateOrganizationSettingsRequest request);
}
