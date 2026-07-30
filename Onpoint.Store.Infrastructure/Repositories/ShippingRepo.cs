

using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Repositories;
using Onpoint.Store.Infrastructure.Data.Context;

namespace Onpoint.Store.Infrastructure.Repositories
{
    public class ShippingRepo : GenericRepository<ProductShipping, int>, IShippingRepo
    {
        public ShippingRepo(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<ProductShipping?> GetByIdAsync(int productId, CancellationToken ct = default)
        {
            return await _dbset.FindAsync(productId, ct);
        }
    }
}
