namespace Onpoint.Store.Application.DTOs.PosSession
{
    public class PosSessionItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImageUrl { get; set; }

        public int? ProductVariantId { get; set; }
        public string? VariantDescription { get; set; }

        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => UnitPrice * Quantity;
    }
}
