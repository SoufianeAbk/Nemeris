using FluentValidation;
using Nemeris.Core.Dtos;

namespace Nemeris.Core.Validation;

public class CategoryUpsertDtoValidator : AbstractValidator<CategoryUpsertDto>
{
    public CategoryUpsertDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);

        RuleFor(x => x.Slug)
            .NotEmpty()
            .MaximumLength(100)
            .Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$")
            .WithMessage("Slug must be lowercase letters, digits and hyphens (e.g. 'home-audio').");

        RuleFor(x => x.Description).MaximumLength(1000);
    }
}
