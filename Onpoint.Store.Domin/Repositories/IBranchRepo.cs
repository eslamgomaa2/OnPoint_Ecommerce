using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Domin.Repositories
{
    public interface IBranchRepo : IGenericRepository<Branch, int>
    {
        Task<int> GetDefaultBranchIdAsync(CancellationToken ct = default);
        Task<bool> ExistsAsync(int id, CancellationToken ct = default);
    }
}