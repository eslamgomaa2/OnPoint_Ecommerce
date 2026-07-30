using Microsoft.EntityFrameworkCore;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Repositories;
using Onpoint.Store.Infrastructure.Data.Context;

namespace Onpoint.Store.Infrastructure.Repositories
{
    public class ProductVariantRepository : GenericRepository<ProductVariant, int>, IProductVariantRepository
    {
        public ProductVariantRepository(ApplicationDbContext context) : base(context) { }

        public async Task<bool> BarcodeExistsAsync(string barcode, int? excludeVariantId = null, CancellationToken ct = default)
        {
            var query = _dbset.Where(v => v.Barcode == barcode && !v.IsDeleted);

            if (excludeVariantId.HasValue)
                query = query.Where(v => v.Id != excludeVariantId.Value);

            return await query.AnyAsync(ct);
        }

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

        public async Task<IReadOnlyList<ProductVariant>> GetAllVariants(string? sku = null, decimal? minPrice = null, decimal? maxPrice = null, decimal? minCost = null, decimal? maxCost = null, bool? isActive = null, int? productId = null, string? searchTerm = null, CancellationToken ct = default)
        {
            IQueryable<ProductVariant> query = _dbset
               .AsNoTracking()
               .Where(v => !v.IsDeleted);

            // 1. Product scope
            if (productId.HasValue && productId.Value > 0)
                query = query.Where(v => v.ProductId == productId.Value);


            if (!string.IsNullOrWhiteSpace(sku))
            {
                var term = sku.Trim().ToLower();
                query = query.Where(v => v.Sku.ToLower().Contains(term));
            }


            if (minPrice.HasValue)
                query = query.Where(v => v.Price >= minPrice.Value);
            if (maxPrice.HasValue)
                query = query.Where(v => v.Price <= maxPrice.Value);


            if (minCost.HasValue)
                query = query.Where(v => v.Cost >= minCost.Value);
            if (maxCost.HasValue)
                query = query.Where(v => v.Cost <= maxCost.Value);


            if (isActive.HasValue)
                query = query.Where(v => v.IsActive == isActive.Value);


            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(v =>
                    v.Sku.ToLower().Contains(term) ||
                    (v.Product != null && v.Product.Name.ToLower().Contains(term)) ||
                    (v.Sku != null && v.Sku.ToLower().Contains(term)));
            }

            return await query
                .Include(v => v.Product)
                .Include(v => v.AttributeValues)
                    .ThenInclude(av => av.ProductAttribute)
                .Include(v => v.Stocks)
                .ToListAsync(ct);
        }
    }
}