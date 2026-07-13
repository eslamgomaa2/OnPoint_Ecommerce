using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Domin.Repositories
{
    public interface IProductRepository : IGenericRepository<Product, int>
    {

        Task<Product?> GetWithDetailsAsync(int id, CancellationToken ct = default);
        Task<List<Product>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);

        Task<(IReadOnlyList<Product> Items, int TotalCount)> GetFilteredPagedAsync(int? categoryId, string? searchTerm, int pageNumber, int pageSize, CancellationToken ct = default);
    }
}
