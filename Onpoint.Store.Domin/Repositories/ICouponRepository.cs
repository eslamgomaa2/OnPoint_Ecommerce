using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Domin.Repositories
{
    public interface ICouponRepository : IGenericRepository<Coupon, int>
    {
        public Task<Coupon?> GetByCodeAsync(string code, CancellationToken ct = default);
        Task<(List<Coupon> Items, int TotalCount)> GetAllPagedAsync(
     string? searchTerm,
     bool? isActive,
     CouponType? discountType,
     int pageNumber,
     int pageSize,
     CancellationToken ct = default);
    }
}
