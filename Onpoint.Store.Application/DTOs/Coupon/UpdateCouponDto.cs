namespace Onpoint.Store.Application.DTOs.Coupon
{
    public class UpdateCouponDto
    {
        public string Code { get; set; } = string.Empty;
        public Domin.Enums.CouponType DiscountType { get; set; }
        public decimal Value { get; set; }
        public decimal MinOrderAmount { get; set; }
        public int MaxUses { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
    }
}
