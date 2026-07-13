using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Application.DTOs.Order
{
    public class CreateOrderDto
    {
        public int AddressId { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public PaymentMethod PaymentMethod { get; set; }
    }
}