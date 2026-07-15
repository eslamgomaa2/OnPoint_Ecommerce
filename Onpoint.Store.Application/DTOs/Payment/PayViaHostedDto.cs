namespace Onpoint.Store.Application.DTOs.Payment
{
    public class PayViaHostedDto
    {
        public int OrderId { get; set; }
        public int PaymentMethodId { get; set; } // اللي جاي من InitiatePayment
    }
}