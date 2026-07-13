using Onpoint.Store.Domin.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Onpoint.Store.Domin.Entities
{
    public class Cart : BaseEntity
    {
        public int UserId { get; set; } = 0;
        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser? User { get; set; }


        public string? AppliedCouponCode { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal DiscountAmount { get; set; } = 0;
        public virtual ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
