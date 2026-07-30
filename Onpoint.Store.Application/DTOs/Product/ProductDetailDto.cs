using Onpoint.Store.Application.DTOs.ProductVariant;
using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Application.DTOs.Product
{
    public class ProductDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Sku { get; set; }

        public string PrimaryImageUrl { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string? BrandName { get; set; }
        public decimal Cost { get; set; }
        public decimal Price { get; set; }
        public decimal DiscountedPrice { get; set; }

        public int TotalStock { get; set; }
        public StockStatus StockStatus { get; set; }
        public ProductStatus Status { get; set; }
        public decimal AverageRating { get; set; }
        public bool IsPopular { get; set; }
        public bool IsInCart { get; set; }
        public bool IsInWishlist { get; set; }
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