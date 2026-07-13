using FluentValidation;
using Onpoint.Store.Application.DTOs.Category;

namespace Onpoint.Store.Application.Validators.Category
{
    public class CreateCategoryDtoValidator : AbstractValidator<CreateCategoryDto>
    {
        public CreateCategoryDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Category Name is required.")
                .MaximumLength(100).WithMessage("Category Name cannot exceed 100 characters.");

            RuleFor(x => x.Slug)
                .NotEmpty().WithMessage("Slug is required.")
                .MaximumLength(120).WithMessage("Slug cannot exceed 120 characters.")
                .Matches(@"^[a-z0-9]+(?:-[a-z0-9]+)*$").WithMessage("Slug must be a valid URL format (e.g., my-category).");
        }
    }
}
