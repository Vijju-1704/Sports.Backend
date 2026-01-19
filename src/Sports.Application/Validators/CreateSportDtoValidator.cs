using FluentValidation;
using Sports.Application.DTOs.Games;

namespace Sports.Application.Validators;

public class CreateSportDtoValidator : AbstractValidator<CreateSportDto>
{
    public CreateSportDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Sport name is required.")
            .MaximumLength(50).WithMessage("Sport name cannot exceed 50 characters.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Sport type is required.")
            .Must(type => type == "Team" || type == "Individual")
            .WithMessage("Sport type must be either 'Team' or 'Individual'.");

        RuleFor(x => x.IconUrl)
            .MaximumLength(10).WithMessage("Icon should be a simple emoji (max 10 characters).")
            .When(x => !string.IsNullOrEmpty(x.IconUrl));
    }
}
