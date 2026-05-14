using FluentValidation;
using Student_Onboarding_Platform.Models.DTOs.Auth;

namespace Student_Onboarding_Platform.Validators;

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}
