using FluentValidation;
using Student_Onboarding_Platform.Models.DTOs.Auth;

namespace Student_Onboarding_Platform.Validators;

public class VerifyOtpRequestValidator : AbstractValidator<VerifyOtpRequest>
{
    public VerifyOtpRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.OtpCode)
            .NotEmpty().WithMessage("OTP code is required.")
            .MaximumLength(10).WithMessage("OTP code cannot exceed 10 characters.");

        RuleFor(x => x.OtpType)
            .NotEmpty().WithMessage("OTP type is required.");
    }
}
