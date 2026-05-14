using FluentValidation;
using Student_Onboarding_Platform.Models.DTOs.Auth;

namespace Student_Onboarding_Platform.Validators;

public class CheckApprovalStatusRequestValidator : AbstractValidator<CheckApprovalStatusRequest>
{
    public CheckApprovalStatusRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");
    }
}
