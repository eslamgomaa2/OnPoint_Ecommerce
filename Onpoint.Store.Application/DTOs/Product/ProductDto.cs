using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Application.DTOs.Product
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Sku { get; set; }
        public string? PrimaryImageUrl { get; set; }
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

    }
}