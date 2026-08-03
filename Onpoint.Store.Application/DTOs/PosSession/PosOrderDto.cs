using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Application.DTOs.PosSession
{
    public class PosOrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string? InvoiceNumber { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal AmountReceived { get; set; }
        public decimal Change { get; set; }
        public string? CustomerName { get; set; }
        public OrderStatus Status { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public OrderSource Source { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public string CashierName { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }
        public string? PaymentUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<SalesOrderItemDto> Items { get; set; } = new List<SalesOrderItemDto>();
    }
}
