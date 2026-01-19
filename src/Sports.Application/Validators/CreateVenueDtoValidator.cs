using FluentValidation;
using Sports.Application.DTOs.Games;

namespace Sports.Application.Validators;

public class CreateVenueDtoValidator : AbstractValidator<CreateVenueDto>
{
    public CreateVenueDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Venue name is required.")
            .MaximumLength(100).WithMessage("Venue name cannot exceed 100 characters.");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required.")
            .MaximumLength(200).WithMessage("Address cannot exceed 200 characters.");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.")
            .MaximumLength(50).WithMessage("City name cannot exceed 50 characters.");

        RuleFor(x => x.MapsUrl)
            .Must(url => string.IsNullOrEmpty(url) || Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Please enter a valid URL.")
            .When(x => !string.IsNullOrEmpty(x.MapsUrl));

        RuleFor(x => x.Facilities)
            .MaximumLength(500).WithMessage("Facilities description cannot exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Facilities));
    }
}
