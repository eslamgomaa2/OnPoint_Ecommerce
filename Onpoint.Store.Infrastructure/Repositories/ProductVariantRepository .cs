using Microsoft.EntityFrameworkCore;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Repositories;
using Onpoint.Store.Infrastructure.Data.Context;

namespace Onpoint.Store.Infrastructure.Repositories
{
    public class ProductVariantRepository : GenericRepository<ProductVariant, int>, IProductVariantRepository
    {
        public ProductVariantRepository(ApplicationDbContext context) : base(context) { }

        public async Task<ProductVariant?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default)
        {
            return await _dbset
                .Include(v => v.AttributeValues)
                    .ThenInclude(av => av.ProductAttribute)
                .Include(v => v.Stocks)
                    .ThenInclude(s => s.Branch)
                .FirstOrDefaultAsync(v => v.Id == id, ct);
        }

        public async Task<IReadOnlyList<ProductVariant>> GetByProductIdAsync(int productId, CancellationToken ct = default)
            => await _dbset
                .Include(v => v.AttributeValues)
                    .ThenInclude(av => av.ProductAttribute)
                .Where(v => v.ProductId == productId && v.IsActive)
                .ToListAsync(ct);

        public async Task<IReadOnlyList<ProductVariant>> GetByProductIdsAsync(
            IEnumerable<int> productIds,
            CancellationToken ct = default)
            => await _dbset
                .Include(v => v.AttributeValues)
                    .ThenInclude(av => av.ProductAttribute)
                .Where(v => productIds.Contains(v.ProductId) && v.IsActive)
                .ToListAsync(ct);

        public async Task<ProductVariant?> GetBySkuAsync(string sku, CancellationToken ct = default)
            => await _dbset
                .Include(v => v.Product)
                .FirstOrDefaultAsync(v => v.Sku == sku && v.IsActive, ct);

        public async Task<bool> ExistsAsync(int productId, string sku, CancellationToken ct = default)
            => await _dbset.AnyAsync(v => v.ProductId == productId && v.Sku == sku, ct);

        public async Task<bool> HasStockAsync(int variantId, int branchId, CancellationToken ct = default)
            => await _dbset
                .AnyAsync(v => v.Id == variantId &&
                    v.Stocks.Any(s => s.BranchId == branchId && s.AvailableQuantity > 0), ct);
    }
}