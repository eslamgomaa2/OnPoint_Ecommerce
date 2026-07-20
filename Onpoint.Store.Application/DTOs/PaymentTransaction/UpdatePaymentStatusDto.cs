using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Application.DTOs.PaymentTransaction
{
    public class UpdatePaymentStatusDto
    {
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public string? GatewayTransactionId { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime? PaidAt { get; set; }
    }
}