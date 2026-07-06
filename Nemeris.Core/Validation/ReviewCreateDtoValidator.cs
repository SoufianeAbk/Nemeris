using FluentValidation;
using Nemeris.Core.Dtos;

namespace Nemeris.Core.Validation;

public class ReviewCreateDtoValidator : AbstractValidator<ReviewCreateDto>
{
    public ReviewCreateDtoValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();

        RuleFor(x => x.Rating).InclusiveBetween(1, 5);

        RuleFor(x => x.Title).MaximumLength(150);

        RuleFor(x => x.Body).MaximumLength(4000);
    }
}
