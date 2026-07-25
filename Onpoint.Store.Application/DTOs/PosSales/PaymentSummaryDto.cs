namespace Onpoint.Store.Application.DTOs.PosSales
{
    public class PaymentSummaryDto
    {
        public string Method { get; set; } = string.Empty;
        public decimal TotalPaid { get; set; }
        public decimal AmountReceived { get; set; }
        public decimal Change { get; set; }
    }
}
