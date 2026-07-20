
using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Domin.Repositories
{
    public interface IProductAttributeRepository : IGenericRepository<ProductAttribute, int>
    {
        Task<ProductAttribute?> GetByIdWithCategoriesAsync(int id, CancellationToken ct = default);
        Task<List<ProductAttribute>> GetAllWithCategoriesAsync(CancellationToken ct = default);
        Task<bool> KeyExistsAsync(string key, int? excludeId = null, CancellationToken ct = default);
        Task<List<Category>> GetCategoriesByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
        Task<bool> IsInUseAsync(int id, CancellationToken ct = default);
    }
}