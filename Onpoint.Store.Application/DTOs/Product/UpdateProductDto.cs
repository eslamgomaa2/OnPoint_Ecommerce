using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Application.DTOs.Product
{
    public class UpdateProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal Cost { get; set; }
        public int CategoryId { get; set; }
        public int? BrandId { get; set; }
        public bool IsPopular { get; set; }
        public string Sku { get; set; } = string.Empty;
        public ProductStatus Status { get; set; }
    }
}