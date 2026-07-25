namespace Onpoint.Store.Application.DTOs.Order
{
    public class PosSalesListResponse
    {
        public IReadOnlyList<PosSalesListItemDto> Items { get; set; } = new List<PosSalesListItemDto>();
        public int TotalCount { get; set; }
    }
}