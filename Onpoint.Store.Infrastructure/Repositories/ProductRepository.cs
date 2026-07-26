using Microsoft.EntityFrameworkCore;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Repositories;
using Onpoint.Store.Infrastructure.Data.Context;

namespace Onpoint.Store.Infrastructure.Repositories
{
    public class ProductRepository : GenericRepository<Product, int>, IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }


        public async Task<Product?> GetWithDetailsAsync(int id, CancellationToken ct = default)
        {
            var product = await _dbset
                .Where(p => !p.IsDeleted)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Translations)
                .Include(p => p.Images.OrderBy(i => !i.IsPrimary))
                .Include(p => p.Discounts.Where(d => d.IsActive && d.EndDate >= DateTime.UtcNow))
                .Include(p => p.Stocks)
                .Include(p => p.Variants.Where(v => v.IsActive))
                    .ThenInclude(v => v.AttributeValues)
                        .ThenInclude(av => av.ProductAttribute)
                .Include(p => p.Variants.Where(v => v.IsActive))
                    .ThenInclude(v => v.Stocks)
                .Include(p => p.AttributeValues)
                    .ThenInclude(av => av.ProductAttribute)
                .FirstOrDefaultAsync(p => p.Id == id, ct);

            return product;
        }

        public async Task<Product?> GetWithStocksForBranchCheckAsync(int id, CancellationToken ct = default)
        {
            return await _dbset
                .Where(p => !p.IsDeleted)
                .Include(p => p.Stocks)
                .Include(p => p.Variants.Where(v => v.IsActive))
                    .ThenInclude(v => v.Stocks)
                .FirstOrDefaultAsync(p => p.Id == id, ct);
        }
        public async Task<Product?> GetWithFullDetailsForAdminAsync(int id, bool includeDeleted = false, CancellationToken ct = default)
        {
            IQueryable<Product> query = _dbset;

            if (!includeDeleted)
                query = query.Where(p => !p.IsDeleted);

            return await query
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Translations)
                .Include(p => p.Images)
                .Include(p => p.Discounts)
                .Include(p => p.Stocks)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.AttributeValues)
                        .ThenInclude(av => av.ProductAttribute)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.Stocks)
                .Include(p => p.AttributeValues)
                    .ThenInclude(av => av.ProductAttribute)
                .FirstOrDefaultAsync(p => p.Id == id, ct);
        }

        public async Task<List<Product>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default)
        {
            return await _dbset
                .Where(p => !p.IsDeleted && ids.Contains(p.Id))
                .ToListAsync(ct);
        }

        public async Task<Product?> GetByIdWithVariantsAsync(int id, CancellationToken ct = default)
        {
            return await _dbset
                .Where(p => !p.IsDeleted)
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == id, ct);
        }


        public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetFilteredPagedAsync(
            int? categoryId, string? searchTerm, int? branchId, int pageNumber, int pageSize, CancellationToken ct = default)
        {
            IQueryable<Product> query = _dbset
                .Where(p => !p.IsDeleted)
                .Include(p => p.Images.Where(i => i.IsPrimary))
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Translations)
                .Include(p => p.Stocks)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.Stocks);

            if (categoryId.HasValue && categoryId.Value > 0)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm) ||
                                          p.Sku.Contains(searchTerm) ||
                                          (p.Description != null && p.Description.Contains(searchTerm)));
            }

            if (branchId.HasValue)
            {
                query = query.Where(p =>
                    p.Stocks.Any(s => s.BranchId == branchId.Value && s.ProductVariantId == null) ||
                    p.Variants.Any(v => v.IsActive && v.Stocks.Any(s => s.BranchId == branchId.Value)));
            }

            int totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(p => p.IsPopular)
                .ThenBy(p => p.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }

        public async Task<bool> SkuExistsAsync(string sku, int? excludeProductId = null, CancellationToken ct = default)
        {
            return await _dbset.AnyAsync(p => p.Sku == sku && (!excludeProductId.HasValue || p.Id != excludeProductId.Value), ct);
        }

        public async Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default)
        {
            return await _dbset
                .Where(p => !p.IsDeleted)
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Sku == sku, ct);
        }

        public async Task<List<Product>> SearchBySkuAsync(string skuTerm, CancellationToken ct = default)
        {
            return await _dbset
                .Where(p => !p.IsDeleted && (p.Sku.Contains(skuTerm) || p.Variants.Any(v => v.Sku.Contains(skuTerm))))
                .Include(p => p.Variants)
                .ToListAsync(ct);
        }


        public async Task<(int InStock, int LowStock, int OutOfStock, int Total)> GetStockCountsAsync(int? branchId, CancellationToken ct = default)
        {
            var projection = await _dbset
                .Where(p => !p.IsDeleted)
                .Select(p => new
                {
                    HasActiveVariants = p.Variants.Any(v => v.IsActive),
                    TotalQty = p.Variants.Any(v => v.IsActive)
                        ? p.Variants.Where(v => v.IsActive)
                            .SelectMany(v => v.Stocks)
                            .Where(s => !branchId.HasValue || s.BranchId == branchId.Value)
                            .Sum(s => (int?)s.Quantity) ?? 0
                        : p.Stocks
                            .Where(s => s.ProductVariantId == null && (!branchId.HasValue || s.BranchId == branchId.Value))
                            .Sum(s => (int?)s.Quantity) ?? 0,
                    MinLevel = p.Variants.Any(v => v.IsActive)
                        ? p.Variants.Where(v => v.IsActive)
                            .SelectMany(v => v.Stocks)
                            .Where(s => !branchId.HasValue || s.BranchId == branchId.Value)
                            .Select(s => (int?)s.MinimumStockLevel).Max() ?? 0
                        : p.Stocks
                            .Where(s => s.ProductVariantId == null && (!branchId.HasValue || s.BranchId == branchId.Value))
                            .Select(s => (int?)s.MinimumStockLevel).Max() ?? 0
                })
                .ToListAsync(ct);

            int inStock = 0, lowStock = 0, outOfStock = 0;

            foreach (var p in projection)
            {
                if (p.TotalQty <= 0)
                    outOfStock++;
                else if (p.TotalQty <= p.MinLevel)
                    lowStock++;
                else
                    inStock++;
            }

            return (inStock, lowStock, outOfStock, projection.Count);
        }
    }
}