namespace Onpoint.Store.Application.DTOs.PosSession
{
    public class PaymentSummaryDto
    {
        public string Method { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
