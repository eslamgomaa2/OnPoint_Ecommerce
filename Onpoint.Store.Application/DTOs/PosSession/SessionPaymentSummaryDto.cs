namespace Onpoint.Store.Application.DTOs.PosSession
{
    public class SessionPaymentSummaryDto
    {
        public string Method { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
