namespace Onpoint.Store.Application.DTOs.Payment
{
    public class MyFatoorahTransactionDetail
    {
        public string TransactionId { get; set; } = string.Empty;
        public string PaymentId { get; set; } = string.Empty;
        public string TransactionStatus { get; set; } = string.Empty;
        public decimal PaidCurrencyValue { get; set; }
        public string? Error { get; set; }
    }
}
