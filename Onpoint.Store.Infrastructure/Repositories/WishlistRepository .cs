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

        public async Task<Wishlist?> GetByUserAndProductIdAsync(int userId, int productId, CancellationToken ct = default)
        {
            return await _dbset
                .FirstOrDefaultAsync(w => w.UserId == userId
                                          && w.ProductId == productId
                                          && !w.IsDeleted, ct);
        }
        public async Task<List<Wishlist>> GetUserWishlistAsync(int userId, CancellationToken ct = default)
        {
            return await _dbset
                .Include(w => w.Product!)
                    .ThenInclude(p => p.Images.Where(img => img.IsPrimary))
                .Include(w => w.Product!)
                    .ThenInclude(p => p.Discounts)
                .Include(w => w.Product!)
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
        public async Task<HashSet<int>> GetWishlistedProductIdsAsync(int userId, IEnumerable<int> productIds, CancellationToken ct = default)
        {
            var idList = productIds.Distinct().ToList();
            if (!idList.Any())
                return new HashSet<int>();

            var result = await _dbset
                .Where(w => w.UserId == userId && !w.IsDeleted && idList.Contains(w.ProductId))
                .Select(w => w.ProductId)
                .Distinct()
                .ToListAsync(ct);

            return result.ToHashSet();
        }
    }
}