namespace Student_Onboarding_Platform.Models.DTOs.Invoice;

public class UpdateInvoiceItemRequest
{
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal TaxPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public int SortOrder { get; set; }
}

/// <summary>
/// Admin-editable invoice fields. Monetary totals (subtotal, tax, discount, grand total,
/// balance due) are recomputed server-side from the line items and fee fields — they are not
/// trusted from the client.
/// </summary>
public class UpdateInvoiceRequest
{
    public string? ReceiptNumber { get; set; }
    public string? TransactionId { get; set; }
    public string? OrderId { get; set; }
    public string? ReferenceNumber { get; set; }

    public DateTime? InvoiceDate { get; set; }
    public DateTime? PaymentDate { get; set; }

    public string? PaymentStatus { get; set; }
    public string? PaymentMethod { get; set; }
    public string? PaymentGateway { get; set; }

    public decimal ConvenienceFee { get; set; }
    public decimal PlatformFee { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal RefundAmount { get; set; }

    public string? Notes { get; set; }
    public string? Terms { get; set; }

    // Organization details on this specific invoice (overrides the snapshot)
    public InvoiceOrganizationDto? Organization { get; set; }
    // Customer details on this specific invoice
    public InvoiceCustomerDto? Customer { get; set; }

    public List<UpdateInvoiceItemRequest> Items { get; set; } = new();
}
