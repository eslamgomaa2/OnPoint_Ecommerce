namespace Onpoint.Store.Application.DTOs.DashBoard
{
    public class TopCustomerDto
    {
        public int CustomerId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int OrderCount { get; set; }
        public decimal TotalSpent { get; set; }
    }
}
