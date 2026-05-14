using FluentValidation;
using Student_Onboarding_Platform.Models.DTOs.Admin;

namespace Student_Onboarding_Platform.Validators;

public class UpdatePaymentRequestValidator : AbstractValidator<UpdatePaymentRequest>
{
    public UpdatePaymentRequestValidator()
    {
        RuleFor(x => x.PaymentStatus)
            .NotEmpty().WithMessage("Payment status is required.")
            .Must(x => x is "Pending" or "Paid" or "Partial" or "Refunded")
            .WithMessage("Payment status must be one of: Pending, Paid, Partial, Refunded.");

        RuleFor(x => x.PaymentAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Payment amount must be a positive value.")
            .When(x => x.PaymentAmount.HasValue);
    }
}
