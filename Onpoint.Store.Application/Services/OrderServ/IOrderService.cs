using BuildingBlocks.Results;
using Onpoint.Store.Application.DTOs.Order;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Application.Services.OrderServ
{
    public interface IOrderService
    {
        Task<ServiceResult<PagedResult<OrderListItemDto>>> GetDashBoardPagedAsync(GetOrdersQueryDto query, int? branchId, CancellationToken ct = default);

        Task<ServiceResult<OrderDetailsDto>> GetOrderDetailsAsync(int id, int? branchId, CancellationToken ct = default);
        Task<ServiceResult<bool>> UpdateOrderAsync(int id, int? branchId, UpdateOrderDto dto, CancellationToken ct = default);
        Task<ServiceResult<PagedResult<OrderDetailsDto>>> GetOrdersPagedAsync(OrdersPaginationRequest query, int? branchId, CancellationToken ct = default);

        //
        Task<ServiceResult<DashBoardSummaryDto>> GetDasheBoardSummaryAsync(int? branchId, CancellationToken ct = default);
        Task<ServiceResult<OrdersSummaryDto>> GetOrdersSummaryAsync(int? branchId, CancellationToken ct = default);


        Task<ServiceResult<OrderDto>> CheckoutAsync(int userId, CreateOrderDto dto);
        Task<ServiceResult<bool>> ProcessPaymentSuccessAsync(int orderId, string transactionId);
        Task<ServiceResult<bool>> CancelOrderAsync(int orderId);
        Task<ServiceResult<bool>> UpdateOrderStatusAsync(int orderId, OrderStatus status);
        Task<ServiceResult<IEnumerable<OrderDto>>> GetUserOrdersAsync(int userId);
        Task<ServiceResult<OrderDto>> GetOrderAsync(int orderId);
        Task FinalizeOrderAsync(Order order, Cart? cart, Coupon? coupon, CancellationToken ct = default);
    }
}