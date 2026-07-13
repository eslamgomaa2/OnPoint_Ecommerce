using BuildingBlocks.Results;
using Onpoint.Store.Application.DTOs.Order;

namespace Onpoint.Store.Application.Services.OrderServ
{
    public interface IOrderService
    {
        Task<ServiceResult<OrderDto>> CheckoutAsync(int userId, CreateOrderDto dto);
        Task<ServiceResult<IEnumerable<OrderDto>>> GetUserOrdersAsync(int userId);
        Task<ServiceResult<bool>> UpdateOrderStatusAsync(int id, UpdateOrderStatusDto dto);
    }
}
