using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Domin.Repositories
{
    public interface ICartRepository : IGenericRepository<Cart, int>
    {
        Task<Cart?> GetUserCartWithItemsAsync(int userId, CancellationToken ct = default);
        Task<HashSet<int>> GetProductIdsInCartAsync(int userId, IEnumerable<int> productIds, CancellationToken ct = default);

    }
}
