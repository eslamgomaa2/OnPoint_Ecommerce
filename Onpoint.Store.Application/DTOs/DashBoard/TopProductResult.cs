namespace Onpoint.Store.Application.DTOs.DashBoard
{
    public class TopProductResult
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
    }
}
