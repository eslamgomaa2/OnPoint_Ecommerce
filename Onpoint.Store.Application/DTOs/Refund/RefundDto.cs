namespace Onpoint.Store.Application.DTOs.Refund
{
    public class RefundDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string? Reason { get; set; }
        public DateTime RefundedAt { get; set; }
        public List<RefundItemDto> Items { get; set; } = new();
    }
}
