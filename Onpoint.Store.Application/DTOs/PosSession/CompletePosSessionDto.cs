using System.ComponentModel.DataAnnotations;

namespace Onpoint.Store.Application.DTOs.PosSession
{
    public class CompletePosSessionDto
    {
        [Required]
        public int PaymentMethodId { get; set; }

        public int? CustomerId { get; set; }

        [Range(0, double.MaxValue)]
        public decimal AmountReceived { get; set; }   // مطلوبة فعلياً لو Cash (PaymentMethodId == 0)
    }
}
