using BuildingBlocks.Results;

namespace Onpoint.Store.Application.DTOs.Discount
{
    public class DiscountFilterRequest : PaginationRequest
    {
        public int? ProductId { get; set; }
    }
}