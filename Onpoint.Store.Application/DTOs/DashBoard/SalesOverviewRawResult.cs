namespace Onpoint.Store.Application.DTOs.DashBoard
{
    public class SalesOverviewRawResult
    {
        public decimal CurrentMonthSales { get; set; }
        public decimal PreviousMonthSales { get; set; }
        public int CurrentMonthOrders { get; set; }
        public int PreviousMonthOrders { get; set; }
        public decimal CurrentMonthRevenue { get; set; }
        public decimal PreviousMonthRevenue { get; set; }
        public int TodayOrders { get; set; }
        public int YesterdayOrders { get; set; }
    }
}
