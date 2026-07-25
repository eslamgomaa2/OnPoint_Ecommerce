using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Application.DTOs.PosSales
{

    public class ExportPosSalesRequestDto
    {
        public string? Search { get; set; }
        public int? BranchId { get; set; }
        public OrderStatus? Status { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

}
