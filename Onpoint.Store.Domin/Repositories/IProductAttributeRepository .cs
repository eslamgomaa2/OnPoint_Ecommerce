
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Domin.Repositories
{
    public interface IProductAttributeRepository : IGenericRepository<ProductAttribute, int>
    {
        Task<ProductAttribute?> GetByIdWithCategoriesAsync(int id, CancellationToken ct = default);
        Task<(IReadOnlyList<ProductAttribute> Items, int TotalCount)> GetAllWithCategoriesAsync(
             int pageNumber,
             int pageSize,
             string? search,
             bool? isActive,
             AttributeValueType? valueType,
             CancellationToken ct = default);
        Task<bool> KeyExistsAsync(string key, int? excludeId = null, CancellationToken ct = default);
        Task<List<Category>> GetCategoriesByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
        Task<bool> IsInUseAsync(int id, CancellationToken ct = default);
        Task<bool> ExistsAsync(int id, CancellationToken ct);
    }
}