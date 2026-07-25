namespace Onpoint.Store.Application.DTOs.PosSales
{
    public class CreateInvoiceDto
    {
        public int OrderId { get; set; }
        public bool IsTaxable { get; set; } = false;
        public string? CustomerTaxNumber { get; set; }
        public string? CustomerCompanyName { get; set; }
    }
}