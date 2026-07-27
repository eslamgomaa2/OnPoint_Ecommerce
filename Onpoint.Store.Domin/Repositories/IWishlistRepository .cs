using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Domin.Repositories
{
    public interface IWishlistRepository : IGenericRepository<Wishlist, int>
    {
        Task<Wishlist?> GetByUserAndProductIdAsync(int userId, int productId, CancellationToken ct = default);
        Task<List<Wishlist>> GetUserWishlistAsync(int userId, CancellationToken ct = default);
        Task<Wishlist?> GetByUserAndProductAsync(int userId, int productId, int? productVariantId, CancellationToken ct = default);
        Task<HashSet<int>> GetWishlistedProductIdsAsync(int userId, IEnumerable<int> productIds, CancellationToken ct = default);

    }
}