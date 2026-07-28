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
        Task<IReadOnlyList<ProductVariant>> GetAllVariants(
string? sku = null,
decimal? minPrice = null,
decimal? maxPrice = null,
decimal? minCost = null,
decimal? maxCost = null,
bool? isActive = null,
int? productId = null,
CancellationToken ct = default);
        Task<bool> SkuExistsAsync(string sku, int? excludeVariantId = null, CancellationToken ct = default);
        Task<bool> BarcodeExistsAsync(string barcode, CancellationToken ct = default);
        Task<List<string>> GetExistingBarcodesAsync(IEnumerable<string> barcodes, CancellationToken ct = default);

    }
}