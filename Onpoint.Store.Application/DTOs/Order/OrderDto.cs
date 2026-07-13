using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Application.DTOs.Order
{
    public class OrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public decimal SubTotal { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public ICollection<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
    }
}
