using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Domin.Repositories
{
    public interface IBrandRepository : IGenericRepository<Brand, int>
    {
        Task<Brand?> GetBySlugAsync(string slug, CancellationToken ct = default);
        Task<List<Product>> GetProductsByBrandIdAsync(int brandId, CancellationToken ct = default);
        Task<bool> SlugExistsAsync(string slug, int? excludeBrandId = null, CancellationToken ct = default);
        Task<bool> NameExistsAsync(string name, int? excludeBrandId = null, CancellationToken ct = default);
        Task<Brand?> GetWithProductsAsync(int id, CancellationToken ct = default);
        Task<(IReadOnlyList<Brand> Items, int TotalCount)> GetFilteredPagedAsync(string? searchTerm, bool? isActive, int pageNumber, int pageSize, CancellationToken ct = default);
    }
}