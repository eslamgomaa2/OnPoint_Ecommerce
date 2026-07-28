using FluentValidation;
using Onpoint.Store.Application.DTOs;
using Onpoint.Store.Application.Validators.Product;
using Onpoint.Store.Domin.Repositories;

public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductDtoValidator(IUnitOfWork unitOfWork)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.CategoryId)
            .GreaterThan(0)
            .WithMessage("Category is required.");


        RuleFor(x => x.Discount)
            .SetValidator(new CreateDiscountDtoValidator()!)
            .When(x => x.Discount != null);

        // ⚠️ REQUIRED: At least one variant
        RuleFor(x => x.Variants)
            .NotNull()
            .WithMessage("Product must have at least one variant.")
            .Must(v => v != null && v.Count > 0)
            .WithMessage("Product must have at least one variant.");

        RuleForEach(x => x.Variants)
            .SetValidator(new CreateProductVariantDtoValidator(unitOfWork));
    }
}