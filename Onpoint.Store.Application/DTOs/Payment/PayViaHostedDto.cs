using System.ComponentModel.DataAnnotations;

namespace Onpoint.Store.Application.DTOs.Payment
{
    public class PayViaHostedDto
    {


        public int? CustomerId { get; set; }

        [Required]
        public int PaymentMethodId { get; set; }

        [Range(0, double.MaxValue)]
        public decimal AmountReceived { get; set; }
    }
}