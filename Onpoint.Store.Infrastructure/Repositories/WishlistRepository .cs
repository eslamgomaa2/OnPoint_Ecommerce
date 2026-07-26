using Microsoft.EntityFrameworkCore;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Repositories;
using Onpoint.Store.Infrastructure.Data.Context;
namespace Onpoint.Store.Infrastructure.Repositories
{
    public class WishlistRepository : GenericRepository<Wishlist, int>, IWishlistRepository
    {
        public WishlistRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<Wishlist>> GetUserWishlistAsync(int userId, CancellationToken ct = default)
        {
            return await _dbset
                .Include(w => w.Product!)
                    .ThenInclude(p => p.Images.Where(img => img.IsPrimary))
                .Include(w => w.Product!)
                    .ThenInclude(p => p.Discounts)
                .Include(w => w.Product!)
                    .ThenInclude(p => p.Stocks)
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<Wishlist?> GetByUserAndProductAsync(int userId, int productId, int? productVariantId, CancellationToken ct = default)
        {
            return await _dbset
                .FirstOrDefaultAsync(w => w.UserId == userId
                                          && w.ProductId == productId
                                          && w.ProductVariantId == productVariantId
                                          && !w.IsDeleted, ct);
        }
    }
}