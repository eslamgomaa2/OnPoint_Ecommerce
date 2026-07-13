using Onpoint.Store.Domin.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Onpoint.Store.Application.DTOs.Cart
{

    public class CartDto
    {
        public int CartId { get; set; }


        public string? AppliedCouponCode { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal DiscountAmount { get; set; } = 0;

        public decimal ShippingCost { get; set; } = 0;

        public PaymentMethod? PaymentMethod { get; set; }
        public string Currency { get; set; } = "USD";

        public int TotalItems => Items.Sum(i => i.Quantity);
        public decimal SubTotal => Items.Sum(i => i.TotalItemPrice);
        public decimal TotalPrice => SubTotal - DiscountAmount + ShippingCost;
        public List<CartItemDto> Items { get; set; } = new();
    }
}