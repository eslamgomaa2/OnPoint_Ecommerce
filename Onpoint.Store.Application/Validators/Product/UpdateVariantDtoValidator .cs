using FluentValidation;
using Onpoint.Store.Application.DTOs.ProductVariant;
using Onpoint.Store.Domin.Enums;
using Onpoint.Store.Domin.Repositories;
using System.Text.RegularExpressions;

namespace Onpoint.Store.Application.Validators.Product
{
    public class UpdateProductVariantDtoValidator : AbstractValidator<UpdateProductVariantDto>
    {
        private static readonly Regex SafeCodeRegex = new(@"^[a-zA-Z0-9_\-]+$", RegexOptions.Compiled);

        public UpdateProductVariantDtoValidator(IUnitOfWork unitOfWork)
        {
            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Price cannot be negative.");

            RuleFor(x => x.Cost)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Cost cannot be negative.");

            RuleFor(x => x)
                .Must(v => v.Cost <= v.Price)
                .WithMessage("Cost cannot be greater than price.")
                .WithName("Cost");

            RuleFor(x => x.Sku)
                .NotEmpty()
                .MaximumLength(50)
                .Matches(SafeCodeRegex)
                .WithMessage("SKU can only contain letters, numbers, hyphens and underscores.")
                .MustAsync(async (dto, sku, ct) =>
                    !await unitOfWork.ProductVariants.SkuExistsAsync(sku!, excludeVariantId: dto.Id, ct))
                .WithMessage(v => $"SKU '{v.Sku}' already exists.")
                .When(x => x.SkuMode == CodeGenerationMode.Manual && !string.IsNullOrEmpty(x.Sku));

            RuleFor(x => x.Barcode)
                .NotEmpty()
                .MaximumLength(50)
                .Matches(SafeCodeRegex)
                .WithMessage("Barcode can only contain letters, numbers, hyphens and underscores.")
                .MustAsync(async (dto, barcode, ct) =>
                    !await unitOfWork.ProductVariants.BarcodeExistsAsync(barcode!, excludeVariantId: dto.Id, ct))
                .WithMessage(v => $"Barcode '{v.Barcode}' already exists.")
                .When(x => x.BarcodeMode == CodeGenerationMode.Manual && !string.IsNullOrEmpty(x.Barcode));

            RuleFor(x => x.QrCodeValue)
                .MaximumLength(100)
                .Matches(SafeCodeRegex)
                .WithMessage("QR code value contains invalid characters.")
                .When(x => x.QrCodeMode == CodeGenerationMode.Manual && !string.IsNullOrEmpty(x.QrCodeValue));

            RuleFor(x => x.Attributes)
                .Must(a => a == null || a.Count <= 30)
                .WithMessage("Too many attributes for a single variant.");

            RuleForEach(x => x.BranchStocks)
                .ChildRules(bs =>
                {
                    bs.RuleFor(b => b.Quantity)
                        .GreaterThanOrEqualTo(0)
                        .WithMessage("Quantity cannot be negative.");

                    bs.RuleFor(b => b.MinimumStockLevel)
                        .GreaterThanOrEqualTo(0)
                        .WithMessage("Minimum stock level cannot be negative.");

                    bs.RuleFor(b => b.BranchId)
                        .GreaterThan(0)
                        .MustAsync(async (id, ct) => await unitOfWork.Branches.ExistsAsync(id, ct))
                        .WithMessage("Branch not found.");
                });
        }
    }
}