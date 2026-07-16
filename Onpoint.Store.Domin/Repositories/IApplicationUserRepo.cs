using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Domin.Repositories
{
    public interface IApplicationUserRepo : IGenericRepository<ApplicationUser, int>
    {

        Task<ApplicationUser?> GetCashierWithBranchAsync(int userId, CancellationToken ct = default);
    }
}
