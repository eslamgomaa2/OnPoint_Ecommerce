using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Domin.Repositories
{
    public interface IStockRepository : IGenericRepository<Stock, int>
    {
        Task<int> GetInStockCountAsync(int? branchId = null, CancellationToken ct = default);
        Task<int> GetOutOfStockCountAsync(int? branchId = null, CancellationToken ct = default);
        Task<int> GetLowStockCountAsync(int? branchId = null, CancellationToken ct = default);
        Task<Stock?> GetByProductVariantAndBranchAsync(int productId, int? productVariantId, int branchId, CancellationToken ct = default);

        Task<int> GetProductsCountByBranchAsync(int? branchId, CancellationToken ct = default);
        Task<int> GetMissingQuantityCountByBranchAsync(int? branchId, CancellationToken ct = default);

        Task<List<Stock>> GetByProductVariantsAndBranchAsync(IEnumerable<(int ProductId, int? ProductVariantId)> keys, int branchId, CancellationToken ct = default);


        Task<IReadOnlyList<Stock>> GetByProductsAndBranchAsync(IEnumerable<int> productIds, int branchId, CancellationToken ct = default);

        Task<Stock?> GetByProductAndBranchAsync(int productId, int? productVariantId, int branchId, CancellationToken ct = default);

        Task<IReadOnlyList<Stock>> GetByProductIdsAsync(IEnumerable<int> productIds, int branchId, CancellationToken ct = default);

        Task<IReadOnlyList<Stock>> GetByVariantIdsAsync(IEnumerable<int> variantIds, int branchId, CancellationToken ct = default);

        Task<IReadOnlyList<Stock>> GetAllByProductAsync(int productId, CancellationToken ct = default);
        Task<Stock?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<Stock>> GetLowStockAsync(int? branchId = null, CancellationToken ct = default);
    }
}