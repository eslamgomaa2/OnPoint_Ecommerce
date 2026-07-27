namespace Onpoint.Store.Application.DTOs.Order
{
    public class OrderItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int? ProductVariantId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImageUrl { get; set; }

        public List<VariantAttributeDto> VariantAttributes { get; set; } = new();

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }


        public decimal TotalPrice => UnitPrice * Quantity;
    }

    public class VariantAttributeDto
    {
        public string AttributeName { get; set; } = string.Empty;
        public string AttributeValue { get; set; } = string.Empty;
    }
}