using Onpoint.Store.Domin.Common;
using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Domin.Entities
{
    public class Refund : BaseEntity
    {
        public int OrderId { get; set; }
        public virtual Order Order { get; set; } = null!;
        public int BranchId { get; set; }
        public virtual Branch Branch { get; set; } = null!;
        public RefundType Type { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Reason { get; set; }
        public DateTime RefundedAt { get; set; }
        public int? ProcessedByUserId { get; set; }

        public virtual ICollection<RefundItem> RefundItems { get; set; } = new List<RefundItem>();
    }
}