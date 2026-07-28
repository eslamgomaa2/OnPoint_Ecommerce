using Microsoft.EntityFrameworkCore;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Repositories;
using Onpoint.Store.Infrastructure.Data.Context;

namespace Onpoint.Store.Infrastructure.Repositories
{
    public class AppSettingsRepository : GenericRepository<AppSettingsInfo, int>, IAppSettingsRepository
    {
        public AppSettingsRepository(ApplicationDbContext context) : base(context) { }

        public async Task<AppSettingsInfo?> GetCurrentAsync(CancellationToken ct = default)
        {
            return await _dbset.FirstOrDefaultAsync(ct);
        }
    }
}