namespace Onpoint.Store.Application.DTOs.Payment
{
    public class PaymentStatusDto
    {
        public int OrderId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}