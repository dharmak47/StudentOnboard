using FluentValidation;
using Student_Onboarding_Platform.Models.DTOs.Auth;

namespace Student_Onboarding_Platform.Validators;

public class ResendOtpRequestValidator : AbstractValidator<ResendOtpRequest>
{
    public ResendOtpRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.OtpType)
            .NotEmpty().WithMessage("OTP type is required.");
    }
}
