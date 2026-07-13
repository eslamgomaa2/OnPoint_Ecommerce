using Microsoft.EntityFrameworkCore;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Repositories;
using Onpoint.Store.Infrastructure.Data.Context;

namespace Onpoint.Store.Infrastructure.Repositories
{
    public class CartRepository : GenericRepository<Cart, int>, ICartRepository
    {
        public CartRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Cart?> GetUserCartWithItemsAsync(int userId, CancellationToken ct = default)
        {
            return await _dbset
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Images.Where(img => img.IsPrimary))
                .FirstOrDefaultAsync(c => c.UserId == userId, ct);
        }
    }
}
