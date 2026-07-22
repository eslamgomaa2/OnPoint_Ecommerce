using Onpoint.Store.Application.DTOs.ProductVariant;

namespace Onpoint.Store.Application.DTOs.Product
{
    public class ProductDetailDto : ProductDto
    {
        public string? Description { get; set; }
        public string? Barcode { get; set; }
        public string? BarcodeImagePath { get; set; }
        public string? QrCodeValue { get; set; }
        public string? QrCodeImagePath { get; set; }
        public List<ProductImageDto> Images { get; set; } = new();
        public List<ProductTranslationDto> Translations { get; set; } = new();
        public List<DiscountDto> ActiveDiscounts { get; set; } = new();
        public List<ProductVariantDto> Variants { get; set; } = new();
        public List<ProductAttributeValueDto> Attributes { get; set; } = new();
        public List<VariantStockDto> BranchStock { get; set; } = new();
        public ProductShippingDto? Shipping { get; set; }
    }
}