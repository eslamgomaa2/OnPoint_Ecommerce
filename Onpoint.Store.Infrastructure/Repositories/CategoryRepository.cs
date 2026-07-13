using Microsoft.EntityFrameworkCore;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Repositories;
using Onpoint.Store.Infrastructure.Data.Context;

namespace Onpoint.Store.Infrastructure.Repositories
{

    public class CategoryRepository : GenericRepository<Category, int>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<Category>> GetMainCategoriesAsync(CancellationToken ct = default)
        {
            return await _dbset
                .Where(c => c.ParentCategoryId == null)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync(ct);
        }


        public async Task<Category?> GetWithSubCategoriesAsync(int categoryId, CancellationToken ct = default)
        {
            return await _dbset
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == categoryId, ct);


        }
    }


}
