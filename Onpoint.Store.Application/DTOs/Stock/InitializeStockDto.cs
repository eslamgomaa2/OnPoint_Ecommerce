namespace Onpoint.Store.Application.DTOs.Stock
{
    public class InitializeStockDto
    {

        public int? ProductVariantId { get; set; }

        public int BranchId { get; set; }
        public int Quantity { get; set; }
    }
}
