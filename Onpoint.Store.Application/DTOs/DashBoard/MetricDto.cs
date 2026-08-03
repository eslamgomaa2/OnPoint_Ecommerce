namespace Onpoint.Store.Application.DTOs.DashBoard
{
    public class MetricDto
    {
        public decimal Value { get; set; }
        public decimal Revenue { get; set; }
        public decimal PercentageChange { get; set; }
        public bool IsIncrease { get; set; }
    }
}