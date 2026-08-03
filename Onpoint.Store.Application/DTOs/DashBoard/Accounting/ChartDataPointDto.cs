namespace Onpoint.Store.Application.DTOs.DashBoard.Accounting
{
    public class ChartDataPointDto
    {
        public string Label { get; set; } = string.Empty; // مثل "16 Jan"
        public decimal Value { get; set; }
    }
}
