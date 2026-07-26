using BuildingBlocks.Results;
using Onpoint.Store.Application.DTOs.Order;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Application.Services.OrderServ
{
    public interface IOrderService
    {
        // Dashboard & Management
        Task<ServiceResult<PagedResult<OrderListItemDto>>> GetDashboardPagedAsync(GetOrdersQueryDto query, int? branchId, CancellationToken ct = default);
        Task<ServiceResult<PagedResult<OrderDetailsDto>>> GetOrdersPagedAsync(OrdersPaginationRequest query, int? branchId, CancellationToken ct = default);
        Task<ServiceResult<OrderDetailsDto>> GetOrderDetailsAsync(int id, int? branchId, CancellationToken ct = default);
        Task<ServiceResult<DashBoardSummaryDto>> GetDashboardSummaryAsync(int? branchId, CancellationToken ct = default);
        Task<ServiceResult<OrdersSummaryDto>> GetOrdersSummaryAsync(int? branchId, CancellationToken ct = default);
        Task<ServiceResult<bool>> UpdateOrderAsync(int id, int? branchId, UpdateOrderDto dto, CancellationToken ct = default);
        Task<ServiceResult<bool>> UpdateOrderStatusAsync(int orderId, OrderStatus status, CancellationToken ct = default);

        // User Operations & Checkout
        Task<ServiceResult<OrderDto>> CheckoutAsync(int userId, CreateOrderDto dto, CancellationToken ct = default);
        Task<ServiceResult<bool>> ProcessPaymentSuccessAsync(int orderId, string transactionId, CancellationToken ct = default);
        Task<ServiceResult<bool>> CancelOrderAsync(int orderId, CancellationToken ct = default);
        Task<ServiceResult<IEnumerable<OrderDto>>> GetUserOrdersAsync(int userId, CancellationToken ct = default);
        Task<ServiceResult<OrderDto>> GetOrderAsync(int orderId, CancellationToken ct = default);
        // في IOrderService
        Task<ServiceResult<List<OrderItemDto>>> GetOrderItemsAsync(int orderId, int userId, CancellationToken ct = default);

        // Internal Processing
        Task FinalizeOrderAsync(Order order, Cart? cart, Coupon? coupon, CancellationToken ct = default);
    }
}