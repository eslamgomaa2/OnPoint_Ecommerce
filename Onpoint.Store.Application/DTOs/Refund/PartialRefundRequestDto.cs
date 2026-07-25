namespace Onpoint.Store.Application.DTOs.Refund
{
    public class PartialRefundRequestDto
    {
        public string? Reason { get; set; }
        public List<RefundItemRequestDto> Items { get; set; } = new();
    }

   
}