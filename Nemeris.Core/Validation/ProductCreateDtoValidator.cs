using FluentValidation;
using Nemeris.Core.Dtos;

namespace Nemeris.Core.Validation;

public class ProductCreateDtoValidator : AbstractValidator<ProductCreateDto>
{
    public ProductCreateDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);

        RuleFor(x => x.Slug)
            .NotEmpty()
            .MaximumLength(200)
            .Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$")
            .WithMessage("Slug must be lowercase letters, digits and hyphens (e.g. 'wireless-mouse').");

        RuleFor(x => x.Sku).NotEmpty().MaximumLength(50);

        RuleFor(x => x.Price).GreaterThan(0);

        RuleFor(x => x.CompareAtPrice)
            .GreaterThan(x => x.Price)
            .When(x => x.CompareAtPrice.HasValue)
            .WithMessage("Compare-at price must be higher than the sale price.");

        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);

        RuleFor(x => x.CategoryId).NotEmpty();
    }
}
