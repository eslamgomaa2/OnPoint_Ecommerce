using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Domin.Repositories
{

    public interface ICategoryRepository : IGenericRepository<Category, int>
    {

        Task<IReadOnlyList<Category>> GetMainCategoriesAsync(CancellationToken ct = default);

        Task<Category?> GetWithSubCategoriesAsync(int categoryId, CancellationToken ct = default);
    }

}
