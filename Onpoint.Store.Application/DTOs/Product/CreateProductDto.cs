using Onpoint.Store.Application.DTOs.Product;
using Onpoint.Store.Domin.Enums;
namespace Onpoint.Store.Application.DTOs
{
    public class CreateProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal Cost { get; set; }
        public int CategoryId { get; set; }
        public int Brandid { get; set; }
        public int MinimumStockLevel { get; set; }


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
        public List<CreateProductTranslationDto> Translations { get; set; } = new();

        public List<CreateProductVariantDto>? Variants { get; set; } = new();
        public List<CreateProductAttributeValueDto> Attributes { get; set; } = new();
        public List<VariantBranchStockDto> BranchStocks { get; set; } = new();
        public CreateProductShippingDto? Shipping { get; set; }
    }
}