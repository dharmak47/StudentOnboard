namespace Student_Onboarding_Platform.Models.DTOs.Invoice;

public class OrganizationSettingsDto
{
    public Guid Id { get; set; }
    public string OrgName { get; set; } = string.Empty;
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
    public string CurrencyCode { get; set; } = "INR";
    public string CurrencySymbol { get; set; } = "₹";
    public decimal DefaultTaxPercent { get; set; }
    public string InvoicePrefix { get; set; } = "INV";
    public string? DefaultNotes { get; set; }
    public string? DefaultTerms { get; set; }
    public string? FooterNote { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class UpdateOrganizationSettingsRequest
{
    public string OrgName { get; set; } = string.Empty;
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
    public string CurrencyCode { get; set; } = "INR";
    public string CurrencySymbol { get; set; } = "₹";
    public decimal DefaultTaxPercent { get; set; }
    public string InvoicePrefix { get; set; } = "INV";
    public string? DefaultNotes { get; set; }
    public string? DefaultTerms { get; set; }
    public string? FooterNote { get; set; }
}
