using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Application.DTOs.PosSession
{
    public class PosOrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public OrderSource Source { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public string CashierName { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<PosOrderItemDto> Items { get; set; } = new List<PosOrderItemDto>();
    }
}
