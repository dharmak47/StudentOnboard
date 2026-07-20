namespace Student_Onboarding_Platform.Models.DTOs.Invoice;

public class InvoiceOrganizationDto
{
    public string? Name { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? TaxRegNo { get; set; }
    public string? LogoUrl { get; set; }
    public string? FooterNote { get; set; }
}

public class InvoiceCustomerDto
{
    public Guid CustomerId { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? BillingAddress { get; set; }
}

public class InvoiceItemDto
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal TaxPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal LineTotal { get; set; }
    public int SortOrder { get; set; }
}

public class InvoiceDto
{
    public Guid Id { get; set; }
    public Guid RegistrationId { get; set; }

    public string InvoiceNumber { get; set; } = string.Empty;
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

    public InvoiceOrganizationDto Organization { get; set; } = new();
    public InvoiceCustomerDto Customer { get; set; } = new();
    public List<InvoiceItemDto> Items { get; set; } = new();

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
