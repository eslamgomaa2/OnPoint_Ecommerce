namespace Onpoint.Store.Application.DTOs.Payment
{
    public class ExecutePaymentResultDto
    {
        public string InvoiceId { get; set; } = string.Empty;
        public string PaymentUrl { get; set; } = string.Empty;
    }
}