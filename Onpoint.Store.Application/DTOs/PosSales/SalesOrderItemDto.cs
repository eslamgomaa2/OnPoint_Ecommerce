namespace Onpoint.Store.Application.DTOs
{
    public class SalesOrderItemDto
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImageUrl { get; set; }
        public string? ProductDescription { get; set; }
        public string Sku { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Total { get; set; }
    }
}