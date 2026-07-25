namespace Onpoint.Store.Application.DTOs.Refund
{
    public class RefundItemDto
    {
        public int OrderItemId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
