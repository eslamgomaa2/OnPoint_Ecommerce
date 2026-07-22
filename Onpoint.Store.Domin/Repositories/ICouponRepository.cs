using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Domin.Repositories
{
    public interface ICouponRepository : IGenericRepository<Coupon, int>
    {
        public Task<Coupon?> GetByCodeAsync(string code, CancellationToken ct = default);
    }
}
