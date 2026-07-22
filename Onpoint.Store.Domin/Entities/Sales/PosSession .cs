using Onpoint.Store.Domin.Common;
using Onpoint.Store.Domin.Entities.Identity;
using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Domin.Entities.Sales
{
    public class PosSession : BaseEntity
    {
        public int CashierId { get; set; }
        public virtual ApplicationUser? Cashier { get; set; }
        public int BranchId { get; set; }
        public virtual Branch? Branch { get; set; }
        public PosSessionStatus Status { get; set; } = PosSessionStatus.Active;
        public string? CustomerPhone { get; set; }
        public string? CouponCode { get; set; }
        public decimal DiscountAmount { get; set; }
        public virtual ICollection<PosSessionItem> Items { get; set; } = new List<PosSessionItem>();
        public int? CustomerId { get; set; }
        public virtual Customer? Customer { get; set; }
        public decimal AmountReceived { get; set; }
        public decimal Change { get; set; }
        public int? OrderId { get; set; }
        public virtual Order? Order { get; set; }
    }
}
