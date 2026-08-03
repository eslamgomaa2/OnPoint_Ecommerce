namespace Onpoint.Store.Application.DTOs.DashBoard
{
    public class RevenueBySourceDto
    {
        public decimal PosRevenue { get; set; }
        public decimal WebsiteRevenue { get; set; }
        public decimal TotalRevenue => PosRevenue + WebsiteRevenue;
    }
}
