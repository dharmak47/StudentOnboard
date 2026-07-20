namespace Student_Onboarding_Platform.Models.DTOs.Invoice;

public class InvoiceListItemDto
{
    public Guid Id { get; set; }
    public Guid RegistrationId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string CourseSummary { get; set; } = string.Empty;
    public decimal GrandTotal { get; set; }
    public decimal AmountPaid { get; set; }
    public string CurrencySymbol { get; set; } = "₹";
    public string PaymentStatus { get; set; } = "Paid";
    public DateTime InvoiceDate { get; set; }
    public DateTime? PaymentDate { get; set; }
}
