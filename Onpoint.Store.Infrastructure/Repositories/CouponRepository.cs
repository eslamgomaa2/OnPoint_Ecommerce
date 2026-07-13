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
    }
}
