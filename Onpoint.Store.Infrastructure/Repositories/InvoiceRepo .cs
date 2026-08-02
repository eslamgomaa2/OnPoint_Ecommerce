using Microsoft.EntityFrameworkCore;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Enums;
using Onpoint.Store.Domin.Repositories;
using Onpoint.Store.Infrastructure.Data.Context;

namespace Onpoint.Store.Infrastructure.Repositories
{
    public class InvoiceRepo : GenericRepository<Order, int>, IInvoiceRepo
    {
        public InvoiceRepo(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<(IReadOnlyList<Order> Items, int TotalCount)> GetInvoicesPagedAsync(
     int pageNumber,
     int pageSize,
     string? search,
     DateTime? dateFrom,
     DateTime? dateTo,
     int? branchId,
     OrderSource? orderSource,
     CancellationToken ct = default)
        {
            IQueryable<Order> query = _dbset
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.Cashier)
                .Include(o => o.Branch)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product);

            query = query.Where(o => !string.IsNullOrWhiteSpace(o.InvoiceNumber));

            if (branchId.HasValue)
                query = query.Where(o => o.BranchId == branchId.Value);

            if (orderSource.HasValue)
                query = query.Where(o => o.Source == orderSource.Value);

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
                    ))
                );
            }

            int totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }

        public async Task<Order?> GetInvoiceDetailsAsync(int id, CancellationToken ct = default)
        {
            return await _dbset
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.Cashier)
                .Include(o => o.Branch)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Refunds)
                    .ThenInclude(r => r.RefundItems)
                .FirstOrDefaultAsync(o => o.Id == id && !string.IsNullOrWhiteSpace(o.InvoiceNumber), ct);
        }
    }
}