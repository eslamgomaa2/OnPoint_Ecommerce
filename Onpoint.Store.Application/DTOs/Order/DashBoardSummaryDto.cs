namespace Onpoint.Store.Application.DTOs.Order
{
    public class DashBoardSummaryDto
    {
        public int TotalOrders { get; set; }
        public int NumberOfProducts { get; set; } = 0;
        public int MissingQuantity { get; set; } = 0;
    }
}