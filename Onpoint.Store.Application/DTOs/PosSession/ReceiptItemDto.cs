namespace Onpoint.Store.Application.DTOs.PosSession
{
    public class ReceiptItemDto
    {
        public string Name { get; set; } = string.Empty;
        public string? VariantDescription { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
