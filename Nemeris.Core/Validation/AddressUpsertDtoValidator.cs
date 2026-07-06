using FluentValidation;
using Nemeris.Core.Dtos;

namespace Nemeris.Core.Validation;

public class AddressUpsertDtoValidator : AbstractValidator<AddressUpsertDto>
{
    public AddressUpsertDtoValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Street).NotEmpty().MaximumLength(200);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PostalCode).NotEmpty().MaximumLength(20);

        // ISO 3166-1 alpha-2 ("BE", "NL", "FR") keeps shipping integrations simple.
        RuleFor(x => x.Country).NotEmpty().Length(2)
            .Matches("^[A-Z]{2}$")
            .WithMessage("Country must be a two-letter ISO code, e.g. 'BE'.");

        RuleFor(x => x.PhoneNumber).MaximumLength(30);
    }
}
