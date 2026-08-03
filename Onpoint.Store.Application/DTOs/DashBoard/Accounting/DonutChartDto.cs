namespace Onpoint.Store.Application.DTOs.DashBoard.Accounting
{
    public class DonutChartDto
    {
        public decimal TotalValue { get; set; }
        public List<DonutSliceDto> Slices { get; set; } = new();
    }
}
