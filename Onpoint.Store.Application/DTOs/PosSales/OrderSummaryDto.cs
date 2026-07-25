namespace Onpoint.Store.Application.DTOs.PosSales
{
    public class OrderSummaryDto
    {
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Tax { get; set; }
        public decimal GrandTotal { get; set; }
    }
}
