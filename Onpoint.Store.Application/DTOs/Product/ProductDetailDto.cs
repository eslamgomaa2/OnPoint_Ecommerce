namespace Onpoint.Store.Application.DTOs.Product
{
    public class ProductDetailDto : ProductDto
    {
        public string? Description { get; set; }
        public int StockQuantity { get; set; }
        public List<ProductImageDto> Images { get; set; } = new();
        public List<DiscountDto> ActiveDiscounts { get; set; } = new();
    }
}
