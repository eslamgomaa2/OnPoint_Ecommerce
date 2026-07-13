using Onpoint.Store.Domin.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Onpoint.Store.Domin.Entities.Sales
{

    public class PaymentTransaction : BaseEntity
    {
        public int OrderId { get; set; }
        [ForeignKey(nameof(OrderId))]
        public virtual Order? Order { get; set; }


        public string Provider { get; set; } = string.Empty; // "Knet", "Tap", "BenefitPay", "COD"
        public string? GatewayTransactionId { get; set; } // الـ ID اللي بيرجعه البوابة (مهم جداً لعمل Reconcile)

        public string Status { get; set; } = string.Empty; // "Pending", "Success", "Failed", "Cancelled"


        [Column(TypeName = "decimal(18,3)")]
        public decimal Amount { get; set; }

        public string CurrencyCode { get; set; } = "KWD";

        public string? CardFirstSix { get; set; }
        public string? CardLastFour { get; set; }

        // رسائل الخطأ من البوابة
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }

        public DateTime? PaidAt { get; set; }
    }
}

