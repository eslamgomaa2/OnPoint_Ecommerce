namespace Onpoint.Store.Application.DTOs.Product
{
    public class ProductVariantFilterRequestDto
    {
        public int? ProductId { get; set; }
        public string? Sku { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public decimal? MinCost { get; set; }
        public decimal? MaxCost { get; set; }
        public bool? IsActive { get; set; }
        public string? SearchTerm { get; set; }

    }
}