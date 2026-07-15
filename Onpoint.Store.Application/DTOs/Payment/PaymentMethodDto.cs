namespace Onpoint.Store.Application.DTOs.Payment
{
    public class PaymentMethodDto
    {
        public int PaymentMethodId { get; set; }
        public string PaymentMethodEn { get; set; } = string.Empty;
        public string PaymentMethodAr { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsDirectPayment { get; set; }
        public double ServiceCharge { get; set; }
        public double TotalAmount { get; set; }
        public string PaymentCurrencyIso { get; set; } = string.Empty;
    }
}