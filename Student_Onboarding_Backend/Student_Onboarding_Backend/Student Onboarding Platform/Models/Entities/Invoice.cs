namespace Student_Onboarding_Platform.Models.Entities;

public class Invoice
{
    public Guid Id { get; set; }
    public Guid RegistrationId { get; set; }
    public Guid UserId { get; set; }

    public string InvoiceNumber { get; set; } = string.Empty;
    public int InvoiceYear { get; set; }
    public int SequenceNumber { get; set; }
    public string? ReceiptNumber { get; set; }
    public string? TransactionId { get; set; }
    public string? OrderId { get; set; }
    public string? ReferenceNumber { get; set; }

    public DateTime InvoiceDate { get; set; }
    public DateTime? PaymentDate { get; set; }

    public string PaymentStatus { get; set; } = "Paid";
    public string? PaymentMethod { get; set; }
    public string? PaymentGateway { get; set; }

    public string CurrencyCode { get; set; } = "INR";
    public string CurrencySymbol { get; set; } = "₹";

    public decimal Subtotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal ConvenienceFee { get; set; }
    public decimal PlatformFee { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal BalanceDue { get; set; }
    public decimal RefundAmount { get; set; }

    public string? Notes { get; set; }
    public string? Terms { get; set; }

    // Organization snapshot (frozen at creation)
    public string? OrgName { get; set; }
    public string? OrgAddressLine1 { get; set; }
    public string? OrgAddressLine2 { get; set; }
    public string? OrgCity { get; set; }
    public string? OrgState { get; set; }
    public string? OrgPostalCode { get; set; }
    public string? OrgCountry { get; set; }
    public string? OrgPhone { get; set; }
    public string? OrgEmail { get; set; }
    public string? OrgWebsite { get; set; }
    public string? OrgTaxRegNo { get; set; }
    public string? OrgLogoUrl { get; set; }
    public string? OrgFooterNote { get; set; }

    // Customer snapshot (frozen at creation)
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerBillingAddress { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
