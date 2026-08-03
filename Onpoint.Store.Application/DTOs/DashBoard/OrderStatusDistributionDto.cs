namespace Onpoint.Store.Application.DTOs.DashBoard
{
    public class OrderStatusDistributionDto
    {
        public int TotalOrders { get; set; }
        public decimal TotalValue { get; set; }
        public List<OrderStatusBreakdownDto> Breakdown { get; set; } = new();
    }
}
