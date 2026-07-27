using FluentValidation;
using Onpoint.Store.Application.DTOs;
using Onpoint.Store.Application.Validators.Product;
using Onpoint.Store.Domin.Enums;
using Onpoint.Store.Domin.Repositories;

public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductDtoValidator(IUnitOfWork unitOfWork)
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);

        RuleFor(x => x.Sku)
            .NotEmpty()
            .When(x => x.SkuMode == CodeGenerationMode.Manual)
            .WithMessage("SKU is required when SkuMode is Manual.");

        RuleFor(x => x.Sku)
            .MustAsync(async (sku, ct) => !await unitOfWork.Products.SkuExistsAsync(sku!, null, ct))
            .When(x => x.SkuMode == CodeGenerationMode.Manual && !string.IsNullOrWhiteSpace(x.Sku))
            .WithMessage("SKU already exists.");

        RuleFor(x => x.Barcode)
            .NotEmpty()
            .When(x => x.BarcodeMode == CodeGenerationMode.Manual)
            .WithMessage("Barcode is required when BarcodeMode is Manual.");

        RuleFor(x => x.Discount)
            .SetValidator(new CreateDiscountDtoValidator()!)
            .When(x => x.Discount != null);

        RuleFor(x => x.Variants)
            .NotNull()
            .WithMessage("Product must have at least one variant.")
            .Must(v => v != null && v.Count > 0)
            .WithMessage("Product must have at least one variant.");

        RuleForEach(x => x.Variants).SetValidator(new CreateProductVariantDtoValidator(unitOfWork));
    }
}