using Microsoft.EntityFrameworkCore;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Repositories;
using Onpoint.Store.Infrastructure.Data.Context;

namespace Onpoint.Store.Infrastructure.Repositories
{
    public class DiscountRepo : GenericRepository<Discount, int>, IDiscountRepo
    {
        public DiscountRepo(ApplicationDbContext context) : base(context)
        {
        }
        public async Task<bool> HasOverlappingDiscountAsync(int productId, DateTime startDate, DateTime endDate, CancellationToken ct = default)
        {
            return await _context.Set<Discount>().AnyAsync(d =>
                d.ProductId == productId &&
                d.IsActive &&
                d.StartDate <= endDate &&
                d.EndDate >= startDate, ct);
        }
        public async Task<List<Discount>> GetDiscountsByProductAsync(int productId, CancellationToken ct = default)
        {
            return await _context.Set<Discount>()
                .Where(d => d.ProductId == productId)
                .OrderByDescending(d => d.StartDate)
                .ToListAsync(ct);
        }
        public async Task<(List<Discount> Items, int TotalCount)> GetAllPagedAsync(
           int? productId,
           string? searchTerm,
           bool? isActive,
           int pageNumber,
           int pageSize,
           CancellationToken ct = default)
        {
            var query = _context.Set<Discount>()
                .Include(d => d.Product)
                .AsQueryable();

            if (productId.HasValue)
            {
                query = query.Where(d => d.ProductId == productId.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(d => d.IsActive == isActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                query = query.Where(d =>
                    d.Product != null &&
                    d.Product.Name.Contains(term));
            }

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(d => d.StartDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }
    }
}
