
using FluentValidation;
using Onpoint.Store.Application.DTOs.Product.BranchManger;
using Onpoint.Store.Domin.Enums;
using Onpoint.Store.Domin.Repositories;

namespace Onpoint.Store.Application.Validators.Product
{
    public class CreateProductByBranchManagerDtoValidator : AbstractValidator<CreateProductByBranchManagerDto>
    {
        public CreateProductByBranchManagerDtoValidator(IUnitOfWork unitOfWork)
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

            RuleForEach(x => x.Variants).SetValidator(new CreateProductVariantByBranchManagerDtoValidator(unitOfWork));
        }
    }
}