using Microsoft.EntityFrameworkCore;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Repositories;
using Onpoint.Store.Infrastructure.Data.Context;

namespace Onpoint.Store.Infrastructure.Repositories
{
    public class BrandRepository : GenericRepository<Brand, int>, IBrandRepository
    {
        public BrandRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Brand?> GetBySlugAsync(string slug, CancellationToken ct = default)
        {
            return await _dbset
                .FirstOrDefaultAsync(b => b.Slug == slug, ct);
        }
        public async Task<List<Product>> GetProductsByBrandIdAsync(int brandId, CancellationToken ct = default)
        {
            return await _context.Set<Product>()
                .Where(p => p.BrandId == brandId)
                .Include(p => p.Images.Where(i => i.IsPrimary))
                .Include(p => p.Category)
                .ToListAsync(ct);
        }
        public async Task<bool> SlugExistsAsync(string slug, int? excludeBrandId = null, CancellationToken ct = default)
        {
            return await _dbset.AnyAsync(b => b.Slug == slug && (!excludeBrandId.HasValue || b.Id != excludeBrandId.Value), ct);
        }

        public async Task<bool> NameExistsAsync(string name, int? excludeBrandId = null, CancellationToken ct = default)
        {
            return await _dbset.AnyAsync(b => b.Name == name && (!excludeBrandId.HasValue || b.Id != excludeBrandId.Value), ct);
        }

        public async Task<Brand?> GetWithProductsAsync(int id, CancellationToken ct = default)
        {
            return await _dbset
                .Include(b => b.Products)
                .FirstOrDefaultAsync(b => b.Id == id, ct);
        }

        public async Task<(IReadOnlyList<Brand> Items, int TotalCount)> GetFilteredPagedAsync(
            string? searchTerm, bool? isActive, int pageNumber, int pageSize, CancellationToken ct = default)
        {
            IQueryable<Brand> query = _dbset.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(b => b.Name.Contains(searchTerm));
            }

            if (isActive.HasValue)
            {
                query = query.Where(b => b.IsActive == isActive.Value);
            }

            int totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderBy(b => b.DisplayOrder)
                .ThenBy(b => b.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }
    }
}