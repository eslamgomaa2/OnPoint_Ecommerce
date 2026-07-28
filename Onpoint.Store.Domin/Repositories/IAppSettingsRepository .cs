using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Domin.Repositories
{
    public interface IAppSettingsRepository : IGenericRepository<AppSettingsInfo, int>
    {
        Task<AppSettingsInfo?> GetCurrentAsync(CancellationToken ct = default);
    }
}