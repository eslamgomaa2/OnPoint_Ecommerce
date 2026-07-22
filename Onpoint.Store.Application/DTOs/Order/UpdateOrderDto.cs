using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Application.DTOs.Order
{
    public class UpdateOrderDto
    {
        public OrderStatus Status { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string? Note { get; set; }
    }
}