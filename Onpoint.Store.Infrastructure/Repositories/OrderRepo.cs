using Microsoft.EntityFrameworkCore;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Enums;
using Onpoint.Store.Domin.Repositories;
using Onpoint.Store.Infrastructure.Data.Context;

namespace Onpoint.Store.Infrastructure.Repositories
{
    public class OrderRepo : GenericRepository<Order, int>, IOrderRepo
    {
        public OrderRepo(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Order>> GetUserOrders(int id)
        {
            var orders = await _dbset.Include(o => o.OrderItems)
                 .ThenInclude(oi => oi.Product)
                 .Where(o => o.UserId == id)
                 .ToListAsync();
            return orders;
        }

        public async Task<bool> HasUserReceivedProductAsync(int userId, int productId, CancellationToken ct = default)
        {
            return await _dbset
                .Where(o => o.UserId == userId && o.Status == OrderStatus.Delivered)
                .SelectMany(o => o.OrderItems)
                .AnyAsync(oi => oi.ProductId == productId, ct);
        }
        public async Task<Order?> GetOrderWithItemsAsync(int orderId, CancellationToken ct = default)
        {
            return await _dbset
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId, ct);
        }
    }
}
