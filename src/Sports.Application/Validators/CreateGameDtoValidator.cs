using FluentValidation;
using Sports.Application.DTOs.Games;

namespace Sports.Application.Validators;

public class CreateGameDtoValidator : AbstractValidator<CreateGameDto>
{
    public CreateGameDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Game title is required.")
            .MinimumLength(3).WithMessage("Title must be at least 3 characters.")
            .MaximumLength(100).WithMessage("Title cannot exceed 100 characters.");

        RuleFor(x => x.SportId)
            .GreaterThan(0).WithMessage("Please select a valid sport.");

        RuleFor(x => x.VenueId)
            .GreaterThan(0).WithMessage("Please select a valid venue.");

        RuleFor(x => x.DateTime)
            .GreaterThan(DateTime.UtcNow).WithMessage("Game date and time must be in the future.")
            .LessThan(DateTime.UtcNow.AddYears(1)).WithMessage("Game date cannot be more than 1 year in the future.");

        RuleFor(x => x.DurationMinutes)
            .InclusiveBetween(30, 300).WithMessage("Duration must be between 30 and 300 minutes.");

        RuleFor(x => x.MinPlayers)
            .InclusiveBetween(2, 50).WithMessage("Minimum players must be between 2 and 50.");

        RuleFor(x => x.MaxPlayers)
            .InclusiveBetween(2, 50).WithMessage("Maximum players must be between 2 and 50.")
            .GreaterThanOrEqualTo(x => x.MinPlayers)
            .WithMessage("Maximum players must be greater than or equal to minimum players.");

        RuleFor(x => x.CostPerPerson)
            .GreaterThanOrEqualTo(0).WithMessage("Cost cannot be negative.")
            .When(x => x.CostPerPerson.HasValue);

        RuleFor(x => x.EquipmentNeeded)
            .MaximumLength(500).WithMessage("Equipment description cannot exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.EquipmentNeeded));
    }
}
