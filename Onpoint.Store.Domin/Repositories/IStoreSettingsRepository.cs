using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Repositories;

namespace Onpoint.Store.Domain.Repositories
{
    public interface IStoreSettingsRepository : IGenericRepository<StoreSettings, int>
    {
        Task<List<StoreSettings>> GetAllAsync(CancellationToken ct = default);
        Task<StoreSettings> GetByIdAsync(int id);
        Task<StoreSettings?> GetByKeyAsync(string key, CancellationToken ct = default);
        Task AddRangeAsync(IEnumerable<StoreSettings> settings, CancellationToken ct = default);
        Task UpdateRangeAsync(IEnumerable<StoreSettings> settings, CancellationToken ct = default);
        Task DeleteAllAsync(CancellationToken ct = default);
        Task<int> SaveChangesAsync(CancellationToken ct = default);

    }
}