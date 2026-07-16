using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Domin.Repositories
{
    public interface IStockRepository : IGenericRepository<Stock, int>
    {
        Task<Stock?> GetByProductAndBranchAsync(int productId, int branchId, CancellationToken ct = default);
        Task<IReadOnlyList<Stock>> GetByProductIdsAsync(IEnumerable<int> productIds, int branchId, CancellationToken ct = default);
    }
}