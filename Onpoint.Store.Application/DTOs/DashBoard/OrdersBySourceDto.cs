namespace Onpoint.Store.Application.DTOs.DashBoard
{
    public class OrdersBySourceDto
    {
        public int TotalOrders { get; set; }
        public decimal TotalValue { get; set; }
        public List<OrderSourceBreakdownDto> Breakdown { get; set; } = new();
    }
}
