using Onpoint.Store.Application.DTOs.Product;
using Onpoint.Store.Application.DTOs.ProductVariant;
using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Application.DTOs
{
    public class CreateProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public int BrandId { get; set; }
        public bool IsPopular { get; set; }
        public ProductStatus Status { get; set; } = ProductStatus.Draft;

        public List<CreateProductImageDto> Images { get; set; } = new();
        public CreateDiscountDto? Discount { get; set; }
        public CreateProductShippingDto? Shipping { get; set; }


        public List<CreateProductVariantDto> Variants { get; set; } = new();
    }
}