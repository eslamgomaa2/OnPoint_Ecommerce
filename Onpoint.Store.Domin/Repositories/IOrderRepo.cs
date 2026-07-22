using Onpoint.Store.Application.DTOs.Order;
using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Domin.Repositories
{
    public interface IOrderRepo : IGenericRepository<Order, int>
    {
        Task<IEnumerable<Order>> GetUserOrders(int id);

        Task<bool> HasUserReceivedProductAsync(int userId, int productId, CancellationToken ct = default);
        Task<Order?> GetOrderWithItemsAsync(int orderId, CancellationToken ct = default);
        Task<(IReadOnlyList<Order> Items, int TotalCount)> GetOrdersPagedAsync(int pageNumber, int pageSize, OrderSortBy sortBy, bool descending, int? branchId, CancellationToken ct = default);

        Task<Order?> GetOrderDetailsAsync(int id, CancellationToken ct = default);

        Task<int> GetTotalOrdersCountAsync(int? branchId, CancellationToken ct = default);
        Task<int> GetCompletedOrdersCountAsync(int? branchId, CancellationToken ct = default);
        Task<int> GetPendingOrdersCountAsync(int? branchId, CancellationToken ct = default);
        Task<decimal> GetTotalRevenueAsync(int? branchId, CancellationToken ct = default);
    }

}

