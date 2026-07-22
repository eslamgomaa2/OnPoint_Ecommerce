using Onpoint.Store.Domin.Common;
using Onpoint.Store.Domin.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Onpoint.Store.Domin.Entities.Sales
{

    public class PaymentTransaction : BaseEntity
    {
        public int OrderId { get; set; }
        [ForeignKey(nameof(OrderId))]
        public virtual Order? Order { get; set; }


        public string Provider { get; set; } = string.Empty;
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
        public string? GatewayTransactionId { get; set; }

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;


        [Column(TypeName = "decimal(18,3)")]
        public decimal Amount { get; set; }

        public string CurrencyCode { get; set; } = "KWD";

        public string? CardFirstSix { get; set; }
        public string? CardLastFour { get; set; }


        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }

        public DateTime? PaidAt { get; set; }
    }
}

