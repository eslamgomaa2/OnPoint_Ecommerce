using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Application.DTOs.Order
{
    public class PosSalesFilterRequest
    {
        public string? Search { get; set; }
        public int? BranchId { get; set; }
        public OrderStatus? Status { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public OrderSortBy SortBy { get; set; } = OrderSortBy.Date;
        public bool Descending { get; set; } = true;
    }
}