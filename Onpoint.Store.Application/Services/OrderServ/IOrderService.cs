
using BuildingBlocks.Results;
using Onpoint.Store.Application.DTOs.Order;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Application.Services.OrderServ
{
    public interface IOrderService
    {
        Task<ServiceResult<OrderDto>> CheckoutAsync(int userId, CreateOrderDto dto);
        Task<ServiceResult<IEnumerable<OrderDto>>> GetUserOrdersAsync(int userId);
        Task<ServiceResult<bool>> UpdateOrderStatusAsync(int id, OrderStatus dto);
        Task FinalizeOrderAsync(Order order, Cart? cart, Coupon? coupon, CancellationToken ct = default);
    }
}
