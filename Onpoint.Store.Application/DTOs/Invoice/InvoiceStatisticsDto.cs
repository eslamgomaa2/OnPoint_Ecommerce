
namespace Onpoint.Store.Application.DTOs.Invoice
{
    public class InvoiceStatisticsDto
    {
        public int TotalInvoices { get; set; }
        public int Paid { get; set; }
        public int Pending { get; set; }
        public int Overdue { get; set; }
    }
}