using FluentValidation;
using Student_Onboarding_Platform.Models.DTOs.Admin;

namespace Student_Onboarding_Platform.Validators;

public class DenyStudentRequestValidator : AbstractValidator<DenyStudentRequest>
{
    public DenyStudentRequestValidator()
    {
        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Reason));
    }
}
