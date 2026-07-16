using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Application.DTOs.PosSession
{
    public class CompletePosSessionDto
    {
        public PaymentMethod PaymentMethod { get; set; }
        public string? CustomerPhone { get; set; }
    }
}
