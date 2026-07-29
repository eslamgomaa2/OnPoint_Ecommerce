namespace Onpoint.Store.Application.DTOs.Order
{
    public class UserOrderSummaryDto
    {
        public decimal SubTotal { get; set; }
        public string? CouponCode { get; set; }
        public decimal? CouponDiscountPercentage { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
    }
}