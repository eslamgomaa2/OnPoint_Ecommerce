namespace Onpoint.Store.Application.DTOs.PaymentTransaction
{
    public class UpdatePaymentStatusDto
    {
        public string Status { get; set; } = string.Empty; // "Success", "Failed"
        public string? GatewayTransactionId { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime? PaidAt { get; set; }
    }
}