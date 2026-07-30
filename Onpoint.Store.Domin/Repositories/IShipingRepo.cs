using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Domin.Repositories
{
    public interface IShippingRepo : IGenericRepository<ProductShipping, int>
    {
        Task<ProductShipping?> GetByIdAsync(int productId, CancellationToken ct = default);
    }
}
