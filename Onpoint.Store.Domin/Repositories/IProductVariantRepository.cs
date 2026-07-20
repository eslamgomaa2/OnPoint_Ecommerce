using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Domin.Repositories
{
    public interface IProductVariantRepository : IGenericRepository<ProductVariant, int>
    {
        Task<ProductVariant?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<ProductVariant>> GetByProductIdAsync(int productId, CancellationToken ct = default);
        Task<IReadOnlyList<ProductVariant>> GetByProductIdsAsync(IEnumerable<int> productIds, CancellationToken ct = default);
        Task<ProductVariant?> GetBySkuAsync(string sku, CancellationToken ct = default);
        Task<bool> ExistsAsync(int productId, string sku, CancellationToken ct = default);
        Task<bool> HasStockAsync(int variantId, int branchId, CancellationToken ct = default);
    }
}