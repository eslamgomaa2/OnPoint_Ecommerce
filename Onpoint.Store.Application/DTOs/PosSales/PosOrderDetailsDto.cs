using Onpoint.Store.Application.DTOs.PosSales;
using Onpoint.Store.Application.DTOs.Refund;
using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Application.DTOs.Order
{
    public class PosOrderDetailsDto
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public OrderStatus Status { get; set; }

        public CustomerSummaryDto Customer { get; set; } = null!;
        public CashierSummaryDto Cashier { get; set; } = null!;
        public BranchSummaryDto Branch { get; set; } = null!;
        public List<SalesOrderItemDto> Items { get; set; } = new();
        public OrderSummaryDto Summary { get; set; } = null!;
        public PaymentSummaryDto Payment { get; set; } = null!;
        public List<TimelineEventDto> Timeline { get; set; } = new();
        public List<RefundDto> Refunds { get; set; } = new();
    }


}