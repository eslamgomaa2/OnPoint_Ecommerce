using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Application.DTOs.PaymentTransaction
{
    public class PaymentTransactionDto
    {
        public int Id { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public DateTime? PaidAt { get; set; }
    }
}