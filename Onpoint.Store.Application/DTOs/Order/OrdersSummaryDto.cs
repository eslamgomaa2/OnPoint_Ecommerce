namespace Onpoint.Store.Application.DTOs.Order
{
    public class OrdersSummaryDto
    {
        public int TotalOrders { get; set; }
        public int Completed { get; set; }
        public int Pending { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}