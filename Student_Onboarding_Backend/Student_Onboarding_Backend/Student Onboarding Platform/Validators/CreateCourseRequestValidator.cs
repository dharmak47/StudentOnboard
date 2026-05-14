using FluentValidation;
using Student_Onboarding_Platform.Models.DTOs.Admin;

namespace Student_Onboarding_Platform.Validators;

public class CreateCourseRequestValidator : AbstractValidator<CreateCourseRequest>
{
    public CreateCourseRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Course name is required.")
            .MaximumLength(200).WithMessage("Course name cannot exceed 200 characters.");

        RuleFor(x => x.Fees)
            .GreaterThanOrEqualTo(0).WithMessage("Fees must be a positive value.");

        RuleFor(x => x.OfferPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Offer price must be a positive value.")
            .LessThanOrEqualTo(x => x.Fees).WithMessage("Offer price cannot exceed the regular fees.")
            .When(x => x.OfferPrice.HasValue);

        RuleFor(x => x.Duration)
            .MaximumLength(100).WithMessage("Duration cannot exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.Duration));
    }
}
