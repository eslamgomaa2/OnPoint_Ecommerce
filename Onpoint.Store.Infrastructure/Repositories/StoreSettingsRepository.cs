using Microsoft.EntityFrameworkCore;
using Onpoint.Store.Domain.Repositories;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Infrastructure.Data.Context;

namespace Onpoint.Store.Infrastructure.Repositories
{
    public class StoreSettingsRepository : GenericRepository<StoreSettings, int>, IStoreSettingsRepository
    {

        private readonly DbSet<StoreSettings> _dbSet;

        public StoreSettingsRepository(ApplicationDbContext context) : base(context)
        {
            _dbSet = context.Set<StoreSettings>();
        }

        public async Task<List<StoreSettings>> GetAllAsync(CancellationToken ct = default)
        {
            return await _dbSet.AsNoTracking().ToListAsync(ct);
        }

        public async Task<StoreSettings?> GetByKeyAsync(string key, CancellationToken ct = default)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.Key == key, ct);
        }

        public async Task AddRangeAsync(IEnumerable<StoreSettings> settings, CancellationToken ct = default)
        {
            await _dbSet.AddRangeAsync(settings, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateRangeAsync(IEnumerable<StoreSettings> settings, CancellationToken ct = default)
        {
            _dbSet.UpdateRange(settings);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAllAsync(CancellationToken ct = default)
        {
            var all = await _dbSet.ToListAsync(ct);
            _dbSet.RemoveRange(all);
            await _context.SaveChangesAsync(ct);
        }
        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            return await _context.SaveChangesAsync(ct);
        }
        public async Task<StoreSettings?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }
    }
}