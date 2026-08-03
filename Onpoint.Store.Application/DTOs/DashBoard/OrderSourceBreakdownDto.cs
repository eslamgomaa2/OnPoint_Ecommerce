using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Application.DTOs.DashBoard
{
    public class OrderSourceBreakdownDto
    {
        public OrderSource Source { get; set; }
        public string SourceName { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal TotalValue { get; set; }
        public decimal Percentage { get; set; }
    }
}
