using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Application.DTOs.PosSession
{
    public class SplitPaymentDto
    {
        public PaymentMethod Method { get; set; }
        public decimal Amount { get; set; }
    }
}
