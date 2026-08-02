using Microsoft.EntityFrameworkCore;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Repositories;
using Onpoint.Store.Infrastructure.Data.Context;

namespace Onpoint.Store.Infrastructure.Repositories
{
    public class ProductAttributeRepository : GenericRepository<ProductAttribute, int>, IProductAttributeRepository
    {
        public ProductAttributeRepository(ApplicationDbContext context) : base(context) { }

        public async Task<ProductAttribute?> GetByIdWithCategoriesAsync(int id, CancellationToken ct = default)
            => await _dbset
                .Include(a => a.Categories)
                .FirstOrDefaultAsync(a => a.Id == id, ct);

        public async Task<List<ProductAttribute>> GetAllWithCategoriesAsync(CancellationToken ct = default)
            => await _dbset
                .Include(a => a.Categories)
                .OrderBy(a => a.Name)
                .ToListAsync(ct);

        public async Task<bool> KeyExistsAsync(string key, int? excludeId = null, CancellationToken ct = default)
            => await _dbset.AnyAsync(a =>
                a.Key == key && (!excludeId.HasValue || a.Id != excludeId.Value), ct);

        public async Task<List<Category>> GetCategoriesByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default)
            => await _context.Set<Category>().Where(c => ids.Contains(c.Id)).ToListAsync(ct);
        public async Task<bool> IsInUseAsync(int id, CancellationToken ct = default)
        {
            return await _context.Set<ProductAttributeValue>().AnyAsync(v => v.ProductAttributeId == id, ct)
                || await _context.Set<VariantAttributeValue>().AnyAsync(v => v.ProductAttributeId == id, ct);
        }

        public Task<bool> ExistsAsync(int id, CancellationToken ct)
        {
            return _dbset.AnyAsync(a => a.Id == id, ct);
        }
    }
}