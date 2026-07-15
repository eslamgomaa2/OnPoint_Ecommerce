using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Domin.Repositories
{
    public interface IOrderRepo : IGenericRepository<Order, int>
    {
        Task<IEnumerable<Order>> GetUserOrders(int id);

        Task<bool> HasUserReceivedProductAsync(int userId, int productId, CancellationToken ct = default);
        Task<Order?> GetOrderWithItemsAsync(int orderId, CancellationToken ct = default);
    }
}
