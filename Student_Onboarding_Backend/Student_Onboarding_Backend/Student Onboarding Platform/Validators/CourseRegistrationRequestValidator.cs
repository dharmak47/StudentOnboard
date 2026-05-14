using FluentValidation;
using Student_Onboarding_Platform.Models.DTOs.Student;

namespace Student_Onboarding_Platform.Validators;

public class CourseRegistrationRequestValidator : AbstractValidator<CourseRegistrationRequest>
{
    public CourseRegistrationRequestValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty().WithMessage("Course ID is required.");
    }
}
