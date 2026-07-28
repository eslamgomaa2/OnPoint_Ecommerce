using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Application.DTOs.ProductVariant
{
    public class UpdateProductVariantDto
    {
        public int? Id { get; set; }

        public CodeGenerationMode SkuMode { get; set; } = CodeGenerationMode.Auto;
        public string? Sku { get; set; }

        public CodeGenerationMode? BarcodeMode { get; set; }
        public string? Barcode { get; set; }

        public CodeGenerationMode? QrCodeMode { get; set; }
        public string? QrCodeValue { get; set; }

        public decimal Price { get; set; }
        public decimal Cost { get; set; }
        public bool IsActive { get; set; } = true;

        public List<VariantAttributeValueDto>? Attributes { get; set; }
        public List<VariantBranchStockDto>? BranchStocks { get; set; }
    }
}