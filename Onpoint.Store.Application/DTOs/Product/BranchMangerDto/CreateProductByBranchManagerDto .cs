using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Application.DTOs.Product.BranchManger
{
    public class CreateProductByBranchManagerDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public int? BrandId { get; set; }
        public int MinimumStockLevel { get; set; }
        public bool IsPopular { get; set; }
        public ProductStatus Status { get; set; } = ProductStatus.Draft;

        public List<CreateProductImageDto>? Images { get; set; } = new();
        public CreateDiscountDto? Discount { get; set; }
        public List<CreateProductTranslationDto>? Translations { get; set; } = new();


        public List<CreateProductVariantByBranchManagerDto> Variants { get; set; } = new();

        public CreateProductShippingDto? Shipping { get; set; }
    }
}