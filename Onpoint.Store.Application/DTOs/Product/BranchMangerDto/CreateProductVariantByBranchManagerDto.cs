using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Application.DTOs.Product.BranchManger
{
    public class CreateProductVariantByBranchManagerDto
    {

        public CodeGenerationMode SkuMode { get; set; } = CodeGenerationMode.Auto;
        public string? Sku { get; set; }

        public CodeGenerationMode? BarcodeMode { get; set; }
        public string? Barcode { get; set; }

        public CodeGenerationMode? QrCodeMode { get; set; }
        public string? QrCodeValue { get; set; }
        public decimal Price { get; set; }


        public List<VariantAttributeValueDto>? Attributes { get; set; } = new();
        public List<BranchManagerStockDto>? BranchStocks { get; set; } = new();
    }

}
