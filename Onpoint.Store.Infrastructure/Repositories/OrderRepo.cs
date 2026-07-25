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

        public async Task<Order?> GetPosOrderDetailsAsync(int id, CancellationToken ct = default)
        {
            return await _dbset
                .Include(o => o.Customer)
                .Include(o => o.Cashier)
                .Include(o => o.Branch)
                .Include(o => o.Invoice)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.StatusHistory)
                .Include(o => o.Refunds)
                    .ThenInclude(r => r.RefundItems)
                .FirstOrDefaultAsync(o => o.Id == id, ct);
        }

        public async Task<(IReadOnlyList<Order> Items, int TotalCount)> GetPosSalesPagedAsync(
            int pageNumber, int pageSize,
            string? search,
            OrderStatus? status,
            PaymentMethod? paymentMethod,
            DateTime? dateFrom,
            DateTime? dateTo,
            int? branchId,
            CancellationToken ct = default)
        {
            IQueryable<Order> query = _dbset.AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.Cashier)
                .Include(o => o.Branch)
                .Where(o => o.Source == OrderSource.Pos);

            if (branchId.HasValue)
                query = query.Where(o => o.BranchId == branchId.Value);

            if (status.HasValue)
                query = query.Where(o => o.Status == status.Value);

            if (paymentMethod.HasValue)
                query = query.Where(o => o.PaymentMethod == paymentMethod.Value);

            if (dateFrom.HasValue)
                query = query.Where(o => o.CreatedAt >= dateFrom.Value);

            if (dateTo.HasValue)
                query = query.Where(o => o.CreatedAt <= dateTo.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(o =>
                    (o.InvoiceNumber != null && o.InvoiceNumber.ToLower().Contains(term)) ||
                    (o.Customer != null && (
                        (o.Customer.FName != null && o.Customer.FName.ToLower().Contains(term)) ||
                        (o.Customer.LName != null && o.Customer.LName.ToLower().Contains(term))
                    )) ||
                    (o.Cashier != null && (
                        o.Cashier.FirstName.ToLower().Contains(term) ||
                        o.Cashier.UserName.ToLower().Contains(term)
                    ))
                );
            }

            int totalCount = await query.CountAsync(ct);

            query = query.OrderByDescending(o => o.CreatedAt);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }

        public async Task<Order?> GetFullOrderDetailsAsync(int id, CancellationToken ct = default)
        {
            return await _dbset
                .Include(o => o.Customer)
                .Include(o => o.Cashier)
                .Include(o => o.Branch)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.Stocks)
                .Include(o => o.StatusHistory.OrderBy(sh => sh.EventTime))
                .Include(o => o.Refunds)
                    .ThenInclude(r => r.RefundItems)
                .FirstOrDefaultAsync(o => o.Id == id && o.Source == OrderSource.Pos, ct);
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
                OrderSortBy.OrderNumber => q => descending ? q.OrderByDescending(o => o.Id) : q.OrderBy(o => o.Id),
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

        public async Task<Order?> GetOrderViaInvoiceNumber(string InvoiceNumber)
        {
            return await _dbset.FirstOrDefaultAsync(o => o.InvoiceNumber == InvoiceNumber);
        }
    }
}


