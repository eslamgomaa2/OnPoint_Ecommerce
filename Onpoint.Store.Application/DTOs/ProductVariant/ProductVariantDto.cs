namespace Onpoint.Store.Application.DTOs.ProductVariant
{
    public class ProductVariantDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string? Barcode { get; set; }
        public string? BarcodeImagePath { get; set; }
        public string? QrCodeValue { get; set; }
        public string? QrCodeImagePath { get; set; }
        public decimal Price { get; set; }


        public decimal Cost { get; set; }

        public bool IsActive { get; set; }


        public List<VariantStockDto> Stocks { get; set; } = new();
        public List<VariantAttributeValueDto> Attributes { get; set; } = new();
    }
}