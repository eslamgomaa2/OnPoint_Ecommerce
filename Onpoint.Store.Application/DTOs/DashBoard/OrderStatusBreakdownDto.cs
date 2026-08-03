using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Application.DTOs.DashBoard
{
    public class OrderStatusBreakdownDto
    {
        public OrderStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal TotalValue { get; set; }
        public decimal Percentage { get; set; }
    }
}
