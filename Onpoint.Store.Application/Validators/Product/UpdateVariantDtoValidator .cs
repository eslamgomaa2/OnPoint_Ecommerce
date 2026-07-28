using FluentValidation;
using Onpoint.Store.Application.DTOs.ProductVariant;
using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Application.Validators.Product
{
    public class UpdateProductVariantDtoValidator : AbstractValidator<UpdateProductVariantDto>
    {
        public UpdateProductVariantDtoValidator()
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