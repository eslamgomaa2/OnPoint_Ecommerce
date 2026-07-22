using FluentValidation;
using Onpoint.Store.Application.DTOs.Category;

namespace Onpoint.Store.Application.Validators.Category
{
    public class UpdateCategoryDtoValidator : AbstractValidator<UpdateCategoryDto>
    {
        public UpdateCategoryDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Valid Category ID is required.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Category Name is required.")
                .MaximumLength(100).WithMessage("Category Name cannot exceed 100 characters.");

        }
    }
}
