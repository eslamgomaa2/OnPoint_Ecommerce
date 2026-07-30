using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Domin.Repositories
{

    public interface ICategoryRepository : IGenericRepository<Category, int>
    {
        Task<Category> GetCategoryByName(string Name, CancellationToken ct);
        Task<bool> ExistsAsync(int id, CancellationToken ct);

    }

}
