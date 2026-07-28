using Onpoint.Store.Application.DTOs.ProductVariant;
using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Application.DTOs.Product.BranchManger
{
    public class UpdateProductByBranchManagerDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public bool IsPopular { get; set; }
        public ProductStatus Status { get; set; } = ProductStatus.Draft;

        public string? Sku { get; set; }

        public CodeGenerationMode? BarcodeMode { get; set; }
        public string? Barcode { get; set; }

        public CodeGenerationMode? QrCodeMode { get; set; }
        public string? QrCodeValue { get; set; }

        public List<CreateProductImageDto> Images { get; set; } = new();
        public CreateDiscountDto? Discount { get; set; }
        public List<UpdateProductTranslationDto> Translations { get; set; } = new();
        public List<UpdateProductVariantDto> Variants { get; set; } = new();
        public List<CreateProductAttributeValueDto> Attributes { get; set; } = new();
        public List<VariantBranchStockDto> BranchStocks { get; set; } = new();
        public UpdateProductShippingDto? Shipping { get; set; }
    }
}


