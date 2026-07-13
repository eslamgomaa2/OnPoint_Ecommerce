namespace Onpoint.Store.Application.DTOs.PaymentTransaction
{
    public class PaymentTransactionDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string Provider { get; set; } = string.Empty;
        public string? GatewayTransactionId { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; } = "KWD";
        public string? CardFirstSix { get; set; }
        public string? CardLastFour { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime? PaidAt { get; set; }
    }
}