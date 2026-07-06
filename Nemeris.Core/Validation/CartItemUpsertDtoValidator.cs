using FluentValidation;
using Nemeris.Core.Dtos;

namespace Nemeris.Core.Validation;

public class CartItemUpsertDtoValidator : AbstractValidator<CartItemUpsertDto>
{
    public CartItemUpsertDtoValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();

        // 0 is allowed: it means "remove this line". Cap protects against abuse.
        RuleFor(x => x.Quantity).InclusiveBetween(0, 999);
    }
}
