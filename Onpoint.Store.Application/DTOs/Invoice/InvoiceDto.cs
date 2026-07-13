namespace Onpoint.Store.Application.DTOs.Invoice
{
    public class InvoiceDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsTaxable { get; set; }
        public decimal TaxPercentage { get; set; }
        public string? CustomerTaxNumber { get; set; }
        public string? CustomerCompanyName { get; set; }
        public DateTime IssuedAt { get; set; }
        public string? PdfUrl { get; set; }
    }
}