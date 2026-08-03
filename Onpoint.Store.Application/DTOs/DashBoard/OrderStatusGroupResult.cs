using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Application.DTOs.DashBoard
{
    public class OrderStatusGroupResult
    {
        public OrderStatus Status { get; set; }
        public int Count { get; set; }
        public decimal TotalValue { get; set; }
    }
}
