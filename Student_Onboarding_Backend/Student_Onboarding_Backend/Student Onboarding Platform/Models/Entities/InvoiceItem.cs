namespace Student_Onboarding_Platform.Models.Entities;

public class InvoiceItem
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal TaxPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal LineTotal { get; set; }
    public int SortOrder { get; set; }
}
