using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Domin.Repositories
{
    public interface IWishlistRepository : IGenericRepository<Wishlist, int>
    {
        Task<List<Wishlist>> GetUserWishlistAsync(int userId, CancellationToken ct = default);
        Task<Wishlist?> GetByUserAndProductAsync(int userId, int productId, int? productVariantId, CancellationToken ct = default);
    }
}