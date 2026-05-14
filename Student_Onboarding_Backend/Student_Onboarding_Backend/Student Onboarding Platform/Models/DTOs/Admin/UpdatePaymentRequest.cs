namespace Student_Onboarding_Platform.Models.DTOs.Admin;

public class UpdatePaymentRequest
{
    public string PaymentStatus { get; set; } = string.Empty;
    public decimal? PaymentAmount { get; set; }
    public string? Notes { get; set; }
}
