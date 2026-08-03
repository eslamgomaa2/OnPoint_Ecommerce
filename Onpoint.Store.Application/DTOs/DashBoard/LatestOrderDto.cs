using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Application.DTOs.DashBoard
{
    public class LatestOrderDto
    {
        public int OrderId { get; set; }
        public decimal Cost { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public string OrderStatusName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
    }
}
