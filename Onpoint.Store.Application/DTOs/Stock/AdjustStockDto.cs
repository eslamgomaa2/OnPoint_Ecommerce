using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Application.DTOs.Stock
{
    public class AdjustStockDto
    {
        public int ProductId { get; set; }
        public int? ProductVariantId { get; set; }
        public int BranchId { get; set; }
        public int Quantity { get; set; }
        public StockAdjustmentType AdjustmentType { get; set; }
        public string? Reason { get; set; }
    }
}
