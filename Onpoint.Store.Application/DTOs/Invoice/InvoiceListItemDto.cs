// Application/DTOs/Invoice/InvoiceListItemDto.cs
namespace Onpoint.Store.Application.DTOs.Invoice
{
    public class InvoiceListItemDto
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public decimal TotalAmount { get; set; }
        public int OrderId { get; set; }
        public string? CustomerName { get; set; }
        public string? BranchName { get; set; }
        public string? Status { get; set; }
    }
}