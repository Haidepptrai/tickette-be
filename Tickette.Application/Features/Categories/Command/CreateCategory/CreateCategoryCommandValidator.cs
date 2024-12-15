using FluentValidation;

namespace Tickette.Application.Features.Categories.Command.CreateCategory;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop) // Stops further validation if NotEmpty fails
            .NotEmpty()
            .WithMessage("Category name is required.")
            .MaximumLength(50)
            .WithMessage("Category name must not exceed 50 characters.")
            .Matches("^[a-zA-Z0-9 ]*$")
            .WithMessage("Category name must only contain letters, numbers, and spaces.");
    }
}