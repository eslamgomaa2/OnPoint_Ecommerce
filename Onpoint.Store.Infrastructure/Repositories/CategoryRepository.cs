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

        public async Task<Category?> GetCategoryByName(string Name, CancellationToken ct)
        {
            return await _dbset.FirstOrDefaultAsync(c => c.Name == Name, ct);
        }
    }


}
