using FluentValidation;
using Onpoint.Store.Application.DTOs.ProductVariant;
using Onpoint.Store.Domin.Enums;
using Onpoint.Store.Domin.Repositories;

namespace Onpoint.Store.Application.Validators.Product
{
    public class CreateProductVariantDtoValidator : AbstractValidator<CreateProductVariantDto>
    {
        public CreateProductVariantDtoValidator(IUnitOfWork unitOfWork)
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


            RuleForEach(x => x.BranchStocks)
                .ChildRules(stock =>
                {
                    stock.RuleFor(s => s.Quantity)
                        .GreaterThanOrEqualTo(0);
                    stock.RuleFor(s => s.MinimumStockLevel)
                        .GreaterThanOrEqualTo(0);
                });
        }
    }
}