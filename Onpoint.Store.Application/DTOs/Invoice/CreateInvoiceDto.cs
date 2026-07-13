namespace Onpoint.Store.Application.DTOs.Invoice
{
    public class CreateInvoiceDto
    {
        public int OrderId { get; set; }
        public bool IsTaxable { get; set; } = false;
        public string? CustomerTaxNumber { get; set; }
        public string? CustomerCompanyName { get; set; }
    }
}