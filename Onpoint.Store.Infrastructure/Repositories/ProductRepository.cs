using Microsoft.EntityFrameworkCore;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Repositories;
using Onpoint.Store.Infrastructure.Data.Context;

namespace Onpoint.Store.Infrastructure.Repositories
{
    public class ProductRepository : GenericRepository<Product, int>, IProductRepository

    {
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Product?> GetWithDetailsAsync(int id, CancellationToken ct = default)
        {
            var product = await _dbset
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
                .Include(p => p.Stocks)
                .Include(p => p.Variants.Where(v => v.IsActive))
                    .ThenInclude(v => v.Stocks)
                .FirstOrDefaultAsync(p => p.Id == id, ct);
        }
        public async Task<Product?> GetWithFullDetailsForAdminAsync(int id, CancellationToken ct = default)
        {
            return await _dbset
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
                .Where(p => ids.Contains(p.Id))
                .ToListAsync(ct);
        }

        public async Task<Product?> GetByIdWithVariantsAsync(int id, CancellationToken ct = default)
        {
            return await _dbset
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == id, ct);
        }

        public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetFilteredPagedAsync(
            int? categoryId, string? searchTerm, int? branchId, int pageNumber, int pageSize, CancellationToken ct = default)
        {
            IQueryable<Product> query = _dbset.AsQueryable();
            query = query
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
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Sku == sku, ct);
        }

        public async Task<List<Product>> SearchBySkuAsync(string skuTerm, CancellationToken ct = default)
        {
            return await _dbset
                .Where(p => p.Sku.Contains(skuTerm) || p.Variants.Any(v => v.Sku.Contains(skuTerm)))
                .Include(p => p.Variants)
                .ToListAsync(ct);
        }
    }
}