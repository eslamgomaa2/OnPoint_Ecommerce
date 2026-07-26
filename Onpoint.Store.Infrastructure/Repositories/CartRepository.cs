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
                        .ThenInclude(p => p!.Images.Where(img => img.IsPrimary))
                .Include(c => c.Items)
                    .ThenInclude(i => i.ProductVariant)
                        .ThenInclude(v => v!.AttributeValues)
                            .ThenInclude(av => av.ProductAttribute)
                .FirstOrDefaultAsync(c => c.UserId == userId, ct);
        }
        public async Task<HashSet<int>> GetProductIdsInCartAsync(int userId, IEnumerable<int> productIds, CancellationToken ct = default)
        {
            var idList = productIds.Distinct().ToList();
            if (!idList.Any())
                return new HashSet<int>();

            var result = await _context.CartItems
                .Where(ci => ci.Cart.UserId == userId && idList.Contains(ci.ProductId))
                .Select(ci => ci.ProductId)
                .Distinct()
                .ToListAsync(ct);

            return result.ToHashSet();
        }
    }
}