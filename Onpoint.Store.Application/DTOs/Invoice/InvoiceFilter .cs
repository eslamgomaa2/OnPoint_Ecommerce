
using BuildingBlocks.Results;
using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Application.DTOs.Invoice
{
    public class InvoiceFilter : PaginationRequest
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public OrderSource? OrderSource { get; set; }

    }
}