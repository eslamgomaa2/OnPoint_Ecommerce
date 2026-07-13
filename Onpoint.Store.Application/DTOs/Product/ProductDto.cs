namespace Onpoint.Store.Application.DTOs.Product
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public decimal FinalPrice { get; set; }

        public string? PrimaryImageUrl { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public bool IsPopular { get; set; }
    }
}
