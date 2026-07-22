using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Application.DTOs.Order
{
    public class OrderDetailsDto
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime OrderDate { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CashierName { get; set; } = string.Empty;
        public string? BranchName { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
    }
}