namespace Onpoint.Store.Application.DTOs.DashBoard.Accounting
{
    public class DonutSliceDto
    {
        public string Category { get; set; } = string.Empty; // Revenue, Earnings, Expenses, Taxes
        public decimal Value { get; set; }
        public decimal Percentage { get; set; }
    }
}
