using Onpoint.Store.Domin.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Onpoint.Store.Domin.Entities
{
    public class RefundItem : BaseEntity
    {
        public int RefundId { get; set; }
        public virtual Refund Refund { get; set; } = null!;

        public int OrderItemId { get; set; }
        public virtual OrderItem OrderItem { get; set; } = null!;

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount => Quantity * UnitPrice;
    }
}