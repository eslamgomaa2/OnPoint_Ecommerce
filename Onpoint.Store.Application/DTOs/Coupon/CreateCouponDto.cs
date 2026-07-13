using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Application.DTOs.Coupon
{
    public class CreateCouponDto
    {
        public string Code { get; set; } = string.Empty;
        public CouponType DiscountType { get; set; }
        public decimal Value { get; set; }
        public decimal MinOrderAmount { get; set; } = 0;
        public int MaxUses { get; set; } = 100;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
