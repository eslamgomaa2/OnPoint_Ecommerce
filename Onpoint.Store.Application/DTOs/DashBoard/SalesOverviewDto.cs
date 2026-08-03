namespace Onpoint.Store.Application.DTOs.DashBoard
{
    public class SalesOverviewDto
    {
        public MetricDto TotalSales { get; set; } = new();
        public MetricDto TotalOrders { get; set; } = new();
        public MetricDto Revenue { get; set; } = new();
        public MetricDto DailyOrders { get; set; } = new();

        public MetricDto CompletedWebOrders { get; set; } = new();

        public MetricDto PosSales { get; set; } = new();

        public MetricDto TotalDiscountRevenue { get; set; } = new();
    }
}
