namespace Onpoint.Store.Application.DTOs.Product
{
    public class UpdateProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int CategoryId { get; set; }
        public bool IsPopular { get; set; }
        public List<CreateProductImageDto> Images { get; set; } = new();
        public CreateDiscountDto? Discount { get; set; }
    }
}
