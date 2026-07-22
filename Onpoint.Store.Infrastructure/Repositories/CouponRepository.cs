using Microsoft.EntityFrameworkCore;
using Onpoint.Store.Domin.Entities;
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
    }
}
