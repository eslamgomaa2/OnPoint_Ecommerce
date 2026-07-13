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
            return await _dbset
                .Include(p => p.Category)
                .Include(p => p.Images.OrderBy(i => !i.IsPrimary))
                .Include(p => p.Discounts.Where(d => d.IsActive && d.EndDate >= DateTime.UtcNow))
                .FirstOrDefaultAsync(p => p.Id == id, ct);
        }
        public async Task<List<Product>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default)
        {
            return await _dbset
                .Where(p => ids.Contains(p.Id))
                .ToListAsync(ct);
        }

        public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetFilteredPagedAsync(int? categoryId, string? searchTerm, int pageNumber, int pageSize, CancellationToken ct = default)
        {
            IQueryable<Product> query = _dbset.AsQueryable();
            query = query.Include(p => p.Images.Where(i => i.IsPrimary));


            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm) ||
                                         (p.Description != null && p.Description.Contains(searchTerm)));
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
    }
}
