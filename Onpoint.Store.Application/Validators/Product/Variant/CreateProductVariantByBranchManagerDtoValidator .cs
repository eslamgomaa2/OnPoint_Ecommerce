using FluentValidation;
using Onpoint.Store.Application.DTOs.Product.BranchManger;
using Onpoint.Store.Domin.Enums;
using Onpoint.Store.Domin.Repositories;

namespace Onpoint.Store.Application.Validators.Product.Variant
{
    public class CreateProductVariantByBranchManagerDtoValidator : AbstractValidator<CreateProductVariantByBranchManagerDto>
    {
        public CreateProductVariantByBranchManagerDtoValidator(IUnitOfWork unitOfWork)
        {
            RuleFor(x => x.Price)
                .GreaterThan(0)
                .WithMessage("Variant price must be greater than 0.");

            RuleFor(x => x.Cost)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Variant cost must be greater than or equal to 0.");

            RuleFor(x => x.Sku)
                .NotEmpty()
                .When(x => x.SkuMode == CodeGenerationMode.Manual)
                .WithMessage("SKU is required when SkuMode is Manual.");

            RuleFor(x => x.Barcode)
                .NotEmpty()
                .When(x => x.BarcodeMode == CodeGenerationMode.Manual)
                .WithMessage("Barcode is required when BarcodeMode is Manual.");
        }
    }
}