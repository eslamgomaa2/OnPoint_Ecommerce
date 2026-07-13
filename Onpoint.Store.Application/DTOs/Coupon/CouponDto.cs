namespace Onpoint.Store.Application.DTOs.Coupon
{
    public class CouponDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public Domin.Enums.CouponType DiscountType { get; set; }
        public decimal Value { get; set; }
        public decimal MinOrderAmount { get; set; }
        public int MaxUses { get; set; }
        public int UsedCount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
    }
}
