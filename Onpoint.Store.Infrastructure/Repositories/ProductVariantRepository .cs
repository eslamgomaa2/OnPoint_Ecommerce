using Microsoft.EntityFrameworkCore;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Repositories;
using Onpoint.Store.Infrastructure.Data.Context;

namespace Onpoint.Store.Infrastructure.Repositories
{
    public class ProductVariantRepository : GenericRepository<ProductVariant, int>, IProductVariantRepository
    {
        public ProductVariantRepository(ApplicationDbContext context) : base(context) { }

        public async Task<bool> BarcodeExistsAsync(string barcode, CancellationToken ct = default)
    => await _dbset.AnyAsync(v => v.Barcode == barcode && !v.IsDeleted, ct);

        public async Task<List<string>> GetExistingBarcodesAsync(IEnumerable<string> barcodes, CancellationToken ct = default)
            => await _dbset
                .Where(v => barcodes.Contains(v.Barcode!) && !v.IsDeleted)
                .Select(v => v.Barcode!)
                .ToListAsync(ct);
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
        public async Task<IReadOnlyList<ProductVariant>> GetAllVariants(
     string? sku = null,
     decimal? minPrice = null,
     decimal? maxPrice = null,
     decimal? minCost = null,
     decimal? maxCost = null,
     bool? isActive = null,
     int? productId = null,
     CancellationToken ct = default)
        {
            IQueryable<ProductVariant> query = _dbset
                .AsNoTracking()
                .Where(v => !v.IsDeleted); // adjust to match your BaseEntity's actual soft-delete flag

            // 1. Product scope (optional)
            if (productId.HasValue && productId.Value > 0)
                query = query.Where(v => v.ProductId == productId.Value);

            // 2. SKU filter (partial match)
            if (!string.IsNullOrWhiteSpace(sku))
            {
                var term = sku.Trim().ToLower();
                query = query.Where(v => v.Sku.ToLower().Contains(term));
            }

            // 3. Price range
            if (minPrice.HasValue)
                query = query.Where(v => v.Price >= minPrice.Value);
            if (maxPrice.HasValue)
                query = query.Where(v => v.Price <= maxPrice.Value);

            // 4. Cost range
            if (minCost.HasValue)
                query = query.Where(v => v.Cost >= minCost.Value);
            if (maxCost.HasValue)
                query = query.Where(v => v.Cost <= maxCost.Value);

            // 5. Active filter
            if (isActive.HasValue)
                query = query.Where(v => v.IsActive == isActive.Value);

            return await query
                .Include(v => v.Product)
                .Include(v => v.AttributeValues)
                    .ThenInclude(av => av.ProductAttribute)
                .Include(v => v.Stocks)
                .ToListAsync(ct);
        }

        public async Task<bool> ExistsAsync(int productId, string sku, CancellationToken ct = default)
            => await _dbset.AnyAsync(v => v.ProductId == productId && v.Sku == sku, ct);

        public async Task<bool> HasStockAsync(int variantId, int branchId, CancellationToken ct = default)
            => await _dbset
                .AnyAsync(v => v.Id == variantId &&
                    v.Stocks.Any(s => s.BranchId == branchId && s.AvailableQuantity > 0), ct);

        public async Task<bool> SkuExistsAsync(string sku, int? excludeVariantId = null, CancellationToken ct = default)
        {
            return await _dbset.AnyAsync(
                v => v.Sku == sku &&
                     (!excludeVariantId.HasValue || v.Id != excludeVariantId.Value),
                ct);
        }
    }
}