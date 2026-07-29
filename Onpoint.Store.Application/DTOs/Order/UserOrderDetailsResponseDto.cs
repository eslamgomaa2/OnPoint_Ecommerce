namespace Onpoint.Store.Application.DTOs.Order
{
    public class UserOrderDetailsResponseDto
    {
        public List<OrderItemDto> Items { get; set; } = new();
        public UserOrderSummaryDto Summary { get; set; } = new();
    }
}