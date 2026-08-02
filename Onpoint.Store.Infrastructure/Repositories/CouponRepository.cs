using Microsoft.EntityFrameworkCore;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Enums;
using Onpoint.Store.Domin.Repositories;
using Onpoint.Store.Infrastructure.Data.Context;

namespace Onpoint.Store.Infrastructure.Repositories
{
    public class CouponRepository : GenericRepository<Coupon, int>, ICouponRepository
    {
        public CouponRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Coupon?> GetByCodeAsync(string code, CancellationToken ct = default)
        {
            return await _dbset.FirstOrDefaultAsync(c => c.Code == code, ct);
        }
        public async Task<(List<Coupon> Items, int TotalCount)> GetAllPagedAsync(
     string? searchTerm,
     bool? isActive,
     CouponType? discountType,
     int pageNumber,
     int pageSize,
     CancellationToken ct = default)
        {
            var query = _context.Set<Coupon>().AsQueryable();

            if (isActive.HasValue)
            {
                query = query.Where(c => c.IsActive == isActive.Value);
            }

            if (discountType.HasValue)
            {
                query = query.Where(c => c.DiscountType == discountType.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                query = query.Where(c => c.Code.Contains(term));
            }

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }
    }


}
