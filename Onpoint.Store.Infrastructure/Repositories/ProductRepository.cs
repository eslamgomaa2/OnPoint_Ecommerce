using Microsoft.EntityFrameworkCore;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Enums;
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
        public async Task<Product?> GetByIdWithShippingAsync(int id, CancellationToken ct = default)
    => await _dbset
        .Include(p => p.Shipping)
        .FirstOrDefaultAsync(p => p.Id == id, ct);
        public async Task<Product?> GetWithDetailsAsync(int id, CancellationToken ct = default)
        {
            var product = await _dbset
                .Where(p => !p.IsDeleted)
                .Include(p => p.Category)
                .Include(p => p.Shipping)
                .Include(p => p.Brand)
                .Include(p => p.Translations)
                .Include(p => p.Images.OrderBy(i => !i.IsPrimary))
                .Include(p => p.Discounts.Where(d => d.IsActive && d.EndDate >= DateTime.UtcNow))
                .Include(p => p.Variants.Where(v => v.IsActive))
                    .ThenInclude(v => v.AttributeValues)
                        .ThenInclude(av => av.ProductAttribute)
                .Include(p => p.Reviews.Where(r => r.IsApproved))
                .FirstOrDefaultAsync(p => p.Id == id, ct);

            return product;
        }

        public async Task<Product?> GetWithStocksForBranchCheckAsync(int id, CancellationToken ct = default)
        {
            return await _dbset
                .Where(p => !p.IsDeleted)
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
                .Include(p => p.Variants)
                    .ThenInclude(v => v.AttributeValues)
                        .ThenInclude(av => av.ProductAttribute)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.Stocks)
                .Include(p => p.Reviews)
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
                .Include(p => p.Images)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Translations)
                .Include(p => p.Discounts.Where(d => d.IsActive && d.EndDate >= DateTime.UtcNow))
                .Include(p => p.Variants.Where(v => v.IsActive))
                    .ThenInclude(v => v.Stocks)
                .Include(p => p.Reviews.Where(r => r.IsApproved));

            if (categoryId.HasValue && categoryId.Value > 0)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm) ||
                                          p.Variants.Any(v => v.Sku.Contains(searchTerm)) ||
                                          (p.Description != null && p.Description.Contains(searchTerm)));
            }

            if (branchId.HasValue)
            {

                query = query.Where(p =>
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


        public async Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default)
        {

            return await _dbset
                .Where(p => !p.IsDeleted &&
                       p.Variants.Any(v => v.Sku == sku))
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<List<Product>> SearchBySkuAsync(string skuTerm, CancellationToken ct = default)
        {
            return await _dbset
                .Where(p => !p.IsDeleted && p.Variants.Any(v => v.Sku.Contains(skuTerm)))
                .Include(p => p.Variants)
                .ToListAsync(ct);
        }

        public async Task<(int InStock, int LowStock, int OutOfStock, int Total)> GetStockCountsAsync(int? branchId, CancellationToken ct = default)
        {
            var projection = await _dbset
                .Where(p => !p.IsDeleted)
                .Select(p => new
                {

                    TotalQty = p.Variants
                        .Where(v => v.IsActive)
                        .SelectMany(v => v.Stocks)
                        .Where(s => !branchId.HasValue || s.BranchId == branchId.Value)
                        .Sum(s => (int?)s.Quantity) ?? 0,
                    MinLevel = p.Variants
                        .Where(v => v.IsActive)
                        .SelectMany(v => v.Stocks)
                        .Where(s => !branchId.HasValue || s.BranchId == branchId.Value)
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


        public async Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default)
        {
            return await _dbset.AnyAsync(p => p.Slug == slug && !p.IsDeleted, ct);
        }


        public async Task<Product?> GetBySlugAsync(string slug, CancellationToken ct = default)
        {
            return await _dbset
                .Where(p => !p.IsDeleted && p.Slug == slug)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .Include(p => p.Variants.Where(v => v.IsActive))
                    .ThenInclude(v => v.AttributeValues)
                        .ThenInclude(av => av.ProductAttribute)
                .Include(p => p.Variants.Where(v => v.IsActive))
                    .ThenInclude(v => v.Stocks)
                .Include(p => p.Reviews.Where(r => r.IsApproved))
                .FirstOrDefaultAsync(ct);
        }

        public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetFilteredAsync(
     int? categoryId,
     int? minRating,
     decimal? minPrice,
     decimal? maxPrice,
     bool? inStockOnly,
     string? search,
     SortBy sortBy,
     int pageNumber,
     int pageSize,
     ProductStatus? status,
     CancellationToken ct = default)
        {
            IQueryable<Product> query = _dbset.AsNoTrackingWithIdentityResolution()
     .Where(p => !p.IsDeleted)
     .Include(p => p.Category)
     .Include(p => p.Brand)
     .Include(p => p.Images)
     .Include(p => p.Reviews)
     .Include(p => p.Discounts)
     .Include(p => p.Variants)
         .ThenInclude(v => v.Stocks)
             .ThenInclude(s => s.Branch)
     .Include(p => p.Variants)
         .ThenInclude(v => v.AttributeValues)
             .ThenInclude(av => av.ProductAttribute)
     .Include(p => p.Discounts);


            if (status.HasValue)
                query = query.Where(p => p.Status == status.Value);
            // 2. Search Filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(term) ||
                    (p.Description != null && p.Description.ToLower().Contains(term)) ||
                    p.Variants.Any(v =>
                        v.Sku.ToLower().Contains(term) ||
                        (v.Barcode != null && v.Barcode.ToLower().Contains(term)) ||
                        (v.QrCodeValue != null && v.QrCodeValue.ToLower().Contains(term))));
            }

            // 3. Price Filter (on variants)
            if (minPrice.HasValue)
                query = query.Where(p => p.Variants.Any(v => v.Price >= minPrice.Value));

            if (maxPrice.HasValue)
                query = query.Where(p => p.Variants.Any(v => v.Price <= maxPrice.Value));

            // 4. In Stock Filter (Quantity > MinimumStockLevel)
            if (inStockOnly == true)
            {
                query = query.Where(p => p.Variants.Any(v =>
                    v.Stocks.Any(s => s.Quantity > s.MinimumStockLevel)));
            }

            // 5. Rating Filter (Average >= minRating)
            if (minRating.HasValue && minRating.Value > 0)
            {
                var minRatingValue = minRating.Value;
                query = query.Where(p =>
                    p.Reviews.Any() &&
                    p.Reviews.Average(r => (double?)r.Rating) >= minRatingValue);
            }

            // Get total count before sorting/paging
            var totalCount = await query.CountAsync(ct);

            // 6. Sorting
            query = sortBy switch
            {
                SortBy.price_asc => query.OrderBy(p => p.Variants.Min(v => v.Price)),
                SortBy.price_desc => query.OrderByDescending(p => p.Variants.Max(v => v.Price)),
                SortBy.rating => query.OrderByDescending(p =>
                    p.Reviews.Any() ? p.Reviews.Average(r => (double?)r.Rating) : 0),
                SortBy.newest => query.OrderByDescending(p => p.CreatedAt),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            // 7. Pagination
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }
    }
}