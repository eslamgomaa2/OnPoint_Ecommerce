namespace Onpoint.Store.Application.DTOs.DashBoard.Accounting
{
    public class AccountingDashboardDto
    {
        public MetricDto BranchReport { get; set; } = new();
        public MetricDto SalesReport { get; set; } = new();
        public MetricDto EarningsReports { get; set; } = new();
        public MetricDto RevenueReport { get; set; } = new();

        public List<ChartDataPointDto> DailyReports { get; set; } = new();
        public DonutChartDto DonutReport { get; set; } = new();
        public List<MonthlyBarChartDto> MonthlyReports { get; set; } = new();
        public List<ChartDataPointDto> YearlyReports { get; set; } = new();
    }
}
