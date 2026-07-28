using FluentValidation;
using Onpoint.Store.Application.DTOs.Product.BranchManger;
using Onpoint.Store.Domin.Repositories;

namespace Onpoint.Store.Application.Validators.Product
{
    public class CreateProductByBranchManagerDtoValidator : AbstractValidator<CreateProductByBranchManagerDto>
    {
        public CreateProductByBranchManagerDtoValidator(IUnitOfWork unitOfWork)
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.CategoryId)
                .GreaterThan(0)
                .WithMessage("Category is required.");


            RuleFor(x => x.Variants)
                .NotNull()
                .WithMessage("Product must have at least one variant.")
                .Must(v => v != null && v.Count > 0)
                .WithMessage("Product must have at least one variant.");

            RuleForEach(x => x.Variants)
                .SetValidator(new CreateProductVariantByBranchManagerDtoValidator(unitOfWork));
        }
    }
}