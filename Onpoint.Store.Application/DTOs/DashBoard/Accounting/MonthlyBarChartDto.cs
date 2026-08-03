namespace Onpoint.Store.Application.DTOs.DashBoard.Accounting
{
    public class MonthlyBarChartDto
    {
        public string Month { get; set; } = string.Empty; // Jan, Feb, etc.
        public decimal Value { get; set; }
    }
}
