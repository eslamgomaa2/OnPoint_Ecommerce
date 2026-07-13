using Onpoint.Store.Domin.Common;
using Onpoint.Store.Domin.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Onpoint.Store.Domin.Entities
{
    public class Coupon : BaseEntity
    {
        public string Code { get; set; } = string.Empty; // الكود اللي بيكتبه اليوزر (مثلاً: SAVE20)

        public CouponType DiscountType { get; set; } // نسبة ولا مبلغ ثابت؟

        [Column(TypeName = "decimal(18,3)")]
        public decimal Value { get; set; } // لو نسبة تكون 20، لو مبلغ يكون 50

        [Column(TypeName = "decimal(18,3)")]
        public decimal MinOrderAmount { get; set; } = 0; // الحد الأدنى للسلة عشان الكوبون يشتغل

        public int MaxUses { get; set; } = 100; // أقصى عدد مرات يستخدم فيها الكوبون
        public int UsedCount { get; set; } = 0; // عدد المرات اللي استخدم بالفعل

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
