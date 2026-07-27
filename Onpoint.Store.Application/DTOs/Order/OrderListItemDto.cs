using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Application.DTOs.Order
{
    public class OrderListItemDto
    {
        public int Id { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
    }
}