using Microsoft.EntityFrameworkCore;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Repositories;
using Onpoint.Store.Infrastructure.Data.Context;

namespace Onpoint.Store.Infrastructure.Repositories
{
    public class BranchRepo : GenericRepository<Branch, int>, IBranchRepo
    {
        public BranchRepo(ApplicationDbContext context) : base(context) { }

        public async Task<int> GetDefaultBranchIdAsync(CancellationToken ct = default)
        {
            var defaultBranch = await _dbset.FirstOrDefaultAsync(b => b.IsDefault && b.IsActive, ct);

            if (defaultBranch is null)
                throw new InvalidOperationException("Default online branch is not configured.");

            return defaultBranch.Id;
        }

    }
}