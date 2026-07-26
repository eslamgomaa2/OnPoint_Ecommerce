using Microsoft.EntityFrameworkCore;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Repositories;
using Onpoint.Store.Infrastructure.Data.Context;

namespace Onpoint.Store.Infrastructure.Repositories
{
    public class StockRepository : GenericRepository<Stock, int>, IStockRepository
    {
        public StockRepository(ApplicationDbContext context) : base(context) { }


        public async Task<int> GetProductsCountByBranchAsync(int? branchId, CancellationToken ct = default)
        {
            return await _dbset
                .Where(s => s.BranchId == branchId)
                .Select(s => s.ProductId)
                .Distinct()
                .CountAsync(ct);
        }

        public async Task<int> GetMissingQuantityCountByBranchAsync(int? branchId, CancellationToken ct = default)
        {
            return await _dbset
                .Where(s => s.BranchId == branchId && (s.Quantity - s.ReservedQuantity) <= 0)
                .Select(s => s.ProductId)
                .Distinct()
                .CountAsync(ct);
        }
        public async Task<int> GetLowStockCountAsync(int? branchId = null, CancellationToken ct = default)
        {
            var query = _dbset.Where(s => s.Quantity <= s.MinimumStockLevel);

            if (branchId.HasValue)
                query = query.Where(s => s.BranchId == branchId.Value);

            return await query.CountAsync(ct);
        }

        public async Task<int> GetInStockCountAsync(int? branchId = null, CancellationToken ct = default)
        {
            var query = _dbset.Where(s => s.Quantity > 0);

            if (branchId.HasValue)
                query = query.Where(s => s.BranchId == branchId.Value);

            return await query.CountAsync(ct);
        }

        public async Task<int> GetOutOfStockCountAsync(int? branchId = null, CancellationToken ct = default)
        {
            var query = _dbset.Where(s => s.Quantity <= 0);

            if (branchId.HasValue)
                query = query.Where(s => s.BranchId == branchId.Value);

            return await query.CountAsync(ct);
        }
        public async Task<Stock?> GetByProductVariantAndBranchAsync(int productId, int? productVariantId, int branchId, CancellationToken ct = default)
        {
            return await _dbset
                .Where(s => s.ProductId == productId &&
                            s.ProductVariantId == productVariantId &&
                            s.BranchId == branchId &&
                            !s.IsDeleted)
                .FirstOrDefaultAsync(ct);
        }
        public async Task<List<Stock>> GetByProductVariantsAndBranchAsync(IEnumerable<(int ProductId, int? ProductVariantId)> keys, int branchId, CancellationToken ct = default)
        {
            var keyList = keys.ToList();
            if (!keyList.Any())
                return new List<Stock>();

            var productIds = keyList.Select(k => k.ProductId).Distinct().ToList();

            var candidateStocks = await _context.Stocks
                .Where(s => s.BranchId == branchId && productIds.Contains(s.ProductId))
                .ToListAsync(ct);
            var keySet = keyList.ToHashSet();

            return candidateStocks
                .Where(s => keySet.Contains((s.ProductId, s.ProductVariantId)))
                .ToList();
        }

        public async Task<IReadOnlyList<Stock>> GetByProductsAndBranchAsync(IEnumerable<int> productIds, int branchId, CancellationToken ct = default)
            => await _dbset
                .Where(s => productIds.Contains(s.ProductId) && s.BranchId == branchId)
                .ToListAsync(ct);

        [Obsolete("Use GetByProductVariantAndBranchAsync instead")]
        public async Task<Stock?> GetByProductAndBranchAsync(int productId, int? productVariantId, int branchId, CancellationToken ct = default)
            => await GetByProductVariantAndBranchAsync(productId, productVariantId, branchId, ct);

        [Obsolete("Use GetByProductsAndBranchAsync or GetByProductVariantsAndBranchAsync instead")]
        public async Task<IReadOnlyList<Stock>> GetByProductIdsAsync(IEnumerable<int> productIds, int branchId, CancellationToken ct = default)
            => await _dbset
                .Where(s => productIds.Contains(s.ProductId) && s.ProductVariantId == null && s.BranchId == branchId)
                .ToListAsync(ct);

        public async Task<IReadOnlyList<Stock>> GetByVariantIdsAsync(IEnumerable<int> variantIds, int branchId, CancellationToken ct = default)
            => await _dbset
                .Where(s => s.ProductVariantId != null && variantIds.Contains(s.ProductVariantId!.Value) && s.BranchId == branchId)
                .ToListAsync(ct);

        public async Task<IReadOnlyList<Stock>> GetAllByProductAsync(int productId, CancellationToken ct = default)
            => await _dbset
                .Include(s => s.Product)
                .Include(s => s.Branch)
                .Include(s => s.ProductVariant)
                    .ThenInclude(v => v!.AttributeValues)
                .Where(s => s.ProductId == productId)
                .ToListAsync(ct);

        public async Task<Stock?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default)
            => await _dbset
                .Include(s => s.Product)
                .Include(s => s.Branch)
                .Include(s => s.ProductVariant)
                    .ThenInclude(v => v!.AttributeValues)
                .FirstOrDefaultAsync(s => s.Id == id, ct);
        public async Task<IReadOnlyList<Stock>> GetLowStockAsync(int? branchId = null, CancellationToken ct = default)
        {
            var query = _dbset
                .Include(s => s.Product)
                .Include(s => s.Branch)
                .Include(s => s.ProductVariant)
                    .ThenInclude(v => v!.AttributeValues)
                .Where(s => s.Quantity <= s.MinimumStockLevel);

            if (branchId.HasValue)
                query = query.Where(s => s.BranchId == branchId.Value);

            return await query.ToListAsync(ct);
        }

    }
}