using Onpoint.Store.Application.DTOs.Product;
using Onpoint.Store.Domin.Enums;
namespace Onpoint.Store.Application.DTOs
{
    public class CreateProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public bool IsPopular { get; set; }
        public ProductStatus Status { get; set; } = ProductStatus.Draft;


        public CodeGenerationMode SkuMode { get; set; } = CodeGenerationMode.Auto;
        public string? Sku { get; set; }

        public CodeGenerationMode? BarcodeMode { get; set; }
        public string? Barcode { get; set; }

        public CodeGenerationMode? QrCodeMode { get; set; }
        public string? QrCodeValue { get; set; }

        public List<CreateProductImageDto> Images { get; set; } = new();
        public CreateDiscountDto? Discount { get; set; }
        public List<CreateProductVariantDto>? Variants { get; set; } = new();
        public List<CreateProductAttributeValueDto> Attributes { get; set; } = new();
        public List<VariantBranchStockDto> BranchStocks { get; set; } = new();
    }
}