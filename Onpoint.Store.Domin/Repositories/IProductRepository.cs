using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Domin.Repositories
{
    public interface IProductRepository : IGenericRepository<Product, int>
    {
        Task<Product?> GetWithDetailsAsync(int id, CancellationToken ct = default);
        Task<Product?> GetByIdWithShippingAsync(int id, CancellationToken ct = default);
        Task<Product?> GetWithStocksForBranchCheckAsync(int id, CancellationToken ct = default);
        Task<Product?> GetWithFullDetailsForAdminAsync(int id, bool includeDeleted = false, CancellationToken ct = default);
        Task<List<Product>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
        Task<Product?> GetByIdWithVariantsAsync(int id, CancellationToken ct = default);
        Task<(IReadOnlyList<Product> Items, int TotalCount)> GetFilteredPagedAsync(
            int? categoryId, string? searchTerm, int? branchId, int pageNumber, int pageSize, CancellationToken ct = default);


        Task<(IReadOnlyList<Product> Items, int TotalCount)> GetFilteredAsync(
            int? categoryId,
            int? minRating,
            decimal? minPrice,
            decimal? maxPrice,
            bool? inStockOnly,
            string? search,
            SortBy sortBy,
            int pageNumber,
            int pageSize,
            CancellationToken ct = default);
        Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default);
        Task<List<Product>> SearchBySkuAsync(string skuTerm, CancellationToken ct = default);
        Task<(int InStock, int LowStock, int OutOfStock, int Total)> GetStockCountsAsync(int? branchId, CancellationToken ct = default);


        Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default);
        Task<Product?> GetBySlugAsync(string slug, CancellationToken ct = default);
    }
}