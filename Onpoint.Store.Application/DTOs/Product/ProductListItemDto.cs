using Onpoint.Store.Application.DTOs.ProductVariant;

namespace Onpoint.Store.Application.DTOs.Product
{
    public class ProductListItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? PrimaryImageUrl { get; set; }
        public decimal Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public bool IsPopular { get; set; }
        public bool InStock { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? BrandName { get; set; }
        public List<ProductVariantDto> Variants { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }
}
