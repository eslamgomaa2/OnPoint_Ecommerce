using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Domin.Repositories
{
    public interface ICartRepository : IGenericRepository<Cart, int>
    {
        Task<Cart?> GetUserCartWithItemsAsync(int userId, CancellationToken ct = default);
    }
}
