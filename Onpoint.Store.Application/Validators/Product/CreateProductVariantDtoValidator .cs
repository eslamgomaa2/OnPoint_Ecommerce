using FluentValidation;
using Onpoint.Store.Application.DTOs.Product;
using Onpoint.Store.Domin.Enums;
using Onpoint.Store.Domin.Repositories;

namespace Onpoint.Store.Application.Validators.Product
{
    public class CreateProductVariantDtoValidator : AbstractValidator<CreateProductVariantDto>
    {
        public CreateProductVariantDtoValidator(IUnitOfWork unitOfWork)
        {
            RuleFor(x => x.Sku)
                .NotEmpty()
                .When(x => x.SkuMode == CodeGenerationMode.Manual)
                .WithMessage("Variant SKU is required when SkuMode is Manual.");

            RuleFor(x => x.Sku)
                .MustAsync(async (sku, ct) => !await unitOfWork.Products.SkuExistsAsync(sku!, null, ct))
                .When(x => x.SkuMode == CodeGenerationMode.Manual && !string.IsNullOrWhiteSpace(x.Sku))
                .WithMessage("Variant SKU already exists.");

            RuleFor(x => x.Barcode)
                .NotEmpty()
                .When(x => x.BarcodeMode == CodeGenerationMode.Manual)
                .WithMessage("Variant barcode is required when BarcodeMode is Manual.");

            RuleFor(x => x.Price)
                .GreaterThan(0)
                .WithMessage("Variant price must be greater than zero.");



            RuleFor(x => x.Attributes)
                .NotEmpty()
                .WithMessage("A variant must have at least one attribute (e.g. Color, Size).");

            RuleForEach(x => x.Attributes).ChildRules(attr =>
            {
                attr.RuleFor(a => a.ProductAttributeId)
                    .GreaterThan(0)
                    .WithMessage("ProductAttributeId must be a valid attribute reference.");

                attr.RuleFor(a => a.Value)
                    .NotEmpty()
                    .WithMessage("Attribute value cannot be empty.");
            });
        }
    }
}