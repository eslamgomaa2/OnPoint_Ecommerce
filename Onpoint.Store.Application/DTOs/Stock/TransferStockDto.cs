namespace Onpoint.Store.Application.DTOs.Stock
{
    public class TransferStockDto
    {
        public int ProductId { get; set; }
        public int? ProductVariantId { get; set; }
        public int FromBranchId { get; set; }
        public int ToBranchId { get; set; }
        public int Quantity { get; set; }
    }
}
