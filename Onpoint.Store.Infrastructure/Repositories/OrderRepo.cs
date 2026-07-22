using Microsoft.EntityFrameworkCore;
using Onpoint.Store.Application.DTOs.Order;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Enums;
using Onpoint.Store.Domin.Repositories;
using Onpoint.Store.Infrastructure.Data.Context;
using System.Linq.Expressions;

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
                 .Where(o => o.CustomerId == id)
                 .ToListAsync();
            return orders;
        }

        public async Task<bool> HasUserReceivedProductAsync(int userId, int productId, CancellationToken ct = default)
        {
            return await _dbset
                .Where(o => o.CustomerId == userId && o.Status == OrderStatus.Completed)
                .SelectMany(o => o.OrderItems)
                .AnyAsync(oi => oi.ProductId == productId, ct);
        }
        public async Task<Order?> GetOrderWithItemsAsync(int orderId, CancellationToken ct = default)
        {
            return await _dbset
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId, ct);
        }
        public async Task<(IReadOnlyList<Order> Items, int TotalCount)> GetOrdersPagedAsync(int pageNumber, int pageSize, OrderSortBy sortBy, bool descending, int? branchId, CancellationToken ct = default)
        {
            Expression<Func<Order, bool>>? predicate = branchId.HasValue
                ? o => o.BranchId == branchId.Value
                : null;

            Func<IQueryable<Order>, IOrderedQueryable<Order>> orderBy = sortBy switch
            {
                OrderSortBy.OrderNumber => q => descending ? q.OrderByDescending(o => o.OrderNumber) : q.OrderBy(o => o.OrderNumber),
                OrderSortBy.TotalAmount => q => descending ? q.OrderByDescending(o => o.TotalAmount) : q.OrderBy(o => o.TotalAmount),
                OrderSortBy.Status => q => descending ? q.OrderByDescending(o => o.Status) : q.OrderBy(o => o.Status),
                _ => q => descending ? q.OrderByDescending(o => o.CreatedAt) : q.OrderBy(o => o.CreatedAt)
            };

            return await GetPagedAsync(pageNumber, pageSize, predicate: predicate, orderBy: orderBy, ct: ct);
        }

        public async Task<Order?> GetOrderDetailsAsync(int id, CancellationToken ct = default)
        {
            return await _dbset
                .Include(o => o.Customer)
                .Include(o => o.Cashier)
                .Include(o => o.Branch)
                .Include(o => o.Invoice)
                .FirstOrDefaultAsync(o => o.Id == id, ct);
        }



        public async Task<int> GetTotalOrdersCountAsync(int? branchId, CancellationToken ct = default)
        {
            var query = _dbset.AsNoTracking().AsQueryable();
            if (branchId.HasValue)
                query = query.Where(o => o.BranchId == branchId.Value);
            return await query.CountAsync(ct);
        }

        public async Task<int> GetCompletedOrdersCountAsync(int? branchId, CancellationToken ct = default)
        {
            var query = _dbset.AsNoTracking().Where(o => o.Status == OrderStatus.Completed);
            if (branchId.HasValue)
                query = query.Where(o => o.BranchId == branchId.Value);
            return await query.CountAsync(ct);
        }

        public async Task<int> GetPendingOrdersCountAsync(int? branchId, CancellationToken ct = default)
        {
            var query = _dbset.AsNoTracking().Where(o => o.Status == OrderStatus.Pending);
            if (branchId.HasValue)
                query = query.Where(o => o.BranchId == branchId.Value);
            return await query.CountAsync(ct);
        }

        public async Task<decimal> GetTotalRevenueAsync(int? branchId, CancellationToken ct = default)
        {
            var query = _dbset.AsNoTracking().Where(o => o.Status == OrderStatus.Completed);
            if (branchId.HasValue)
                query = query.Where(o => o.BranchId == branchId.Value);
            return await query.SumAsync(o => o.TotalAmount, ct);
        }
    }
}


