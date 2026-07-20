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
    }
}
