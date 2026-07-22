
namespace Onpoint.Store.Application.DTOs.Customer
{
    public class CustomerStatsDto
    {
        public int TotalCustomers { get; set; }
        public int Active { get; set; }
        public int NewThisMonth { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}