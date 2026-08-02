// Application/DTOs/Invoice/InvoiceDetailsDto.cs
namespace Onpoint.Store.Application.DTOs.Invoice
{
    public class InvoiceDetailsDto
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public int OrderId { get; set; }
        public string? CustomerName { get; set; }
        public string? CashierName { get; set; }
        public string? BranchName { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public List<InvoiceItemDto> Items { get; set; } = new();
    }


}