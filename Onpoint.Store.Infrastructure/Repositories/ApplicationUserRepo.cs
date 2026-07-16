using Microsoft.EntityFrameworkCore;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Repositories;
using Onpoint.Store.Infrastructure.Data.Context;

namespace Onpoint.Store.Infrastructure.Repositories
{
    public class ApplicationUserRepo : GenericRepository<ApplicationUser, int>, IApplicationUserRepo
    {
        public ApplicationUserRepo(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<ApplicationUser?> GetCashierWithBranchAsync(int userId, CancellationToken ct = default)
        {
            return await _dbset
                .Include(u => u.Branch)
                .FirstOrDefaultAsync(u => u.Id == userId, ct);
        }
    }
}
