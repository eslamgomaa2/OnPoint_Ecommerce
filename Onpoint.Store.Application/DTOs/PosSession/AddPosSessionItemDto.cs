namespace Onpoint.Store.Application.DTOs.PosSession
{
    public class AddPosSessionItemDto
    {
        public int ProductId { get; set; }
        public int? ProductVariantId { get; set; }
        public int Quantity { get; set; }
    }
}
