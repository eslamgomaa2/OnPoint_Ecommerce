using FluentValidation;
using Onpoint.Store.Application.DTOs;
using Onpoint.Store.Application.DTOs.ProductVariant;
using Onpoint.Store.Application.Validators.Product;
using Onpoint.Store.Domin.Enums;
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
            .WithMessage("Category is required.")

            .MustAsync(async (id, ct) => await unitOfWork.Categories.ExistsAsync(id, ct))
            .WithMessage("Category not found.");

        RuleFor(x => x.BrandId)
            .MustAsync(async (id, ct) => id == 0 || await unitOfWork.Brands.ExistsAsync(id, ct))
            .WithMessage("Brand not found.")
            .When(x => x.BrandId != 0);

        RuleFor(x => x.Discount)
            .SetValidator(new CreateDiscountDtoValidator()!)
            .When(x => x.Discount != null);

        RuleFor(x => x.Variants)
            .NotNull()
            .WithMessage("Product must have at least one variant.")
            .Must(v => v != null && v.Count > 0)
            .WithMessage("Product must have at least one variant.")
            .Must(v => v == null || v.Count <= 50)
            .WithMessage("Cannot create more than 50 variants at once.");

        RuleForEach(x => x.Variants)
            .SetValidator(new CreateProductVariantDtoValidator(unitOfWork));

        RuleFor(x => x.Variants)
            .Must(variants => !HasDuplicateManualSkus(variants))
            .WithMessage("Duplicate manual SKU found within the same request.")
            .When(x => x.Variants != null);

        RuleFor(x => x.Variants)
            .Must(variants => !HasDuplicateManualBarcodes(variants))
            .WithMessage("Duplicate manual barcode found within the same request.")
            .When(x => x.Variants != null);

        RuleForEach(x => x.Variants)
            .Must(v => v.BranchStocks == null ||
                       v.BranchStocks.Select(bs => bs.BranchId).Distinct().Count() == v.BranchStocks.Count)
            .WithMessage("Duplicate BranchId found in BranchStocks for a variant.");

        RuleFor(x => x.Images)
            .Must(images => images == null || images.Count <= 20)
            .WithMessage("Cannot upload more than 20 images.");

        RuleFor(x => x.Images)
            .Must(images => images == null || !images.Any() || images.Count(i => i.IsPrimary) == 1)
            .WithMessage("Exactly one image must be marked as primary.")
            .When(x => x.Images != null && x.Images.Any());
    }

    private static bool HasDuplicateManualSkus(List<CreateProductVariantDto>? variants)
    {
        if (variants == null) return false;
        var skus = variants
            .Where(v => v.SkuMode == CodeGenerationMode.Manual && !string.IsNullOrEmpty(v.Sku))
            .Select(v => v.Sku!.Trim().ToUpperInvariant());
        return skus.GroupBy(s => s).Any(g => g.Count() > 1);
    }

    private static bool HasDuplicateManualBarcodes(List<CreateProductVariantDto>? variants)
    {
        if (variants == null) return false;
        var barcodes = variants
            .Where(v => v.BarcodeMode == CodeGenerationMode.Manual && !string.IsNullOrEmpty(v.Barcode))
            .Select(v => v.Barcode!.Trim());
        return barcodes.GroupBy(b => b).Any(g => g.Count() > 1);
    }
}