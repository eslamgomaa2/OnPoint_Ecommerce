using Microsoft.EntityFrameworkCore;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Enums;
using Onpoint.Store.Domin.Repositories;
using Onpoint.Store.Infrastructure.Data.Context;

namespace Onpoint.Store.Infrastructure.Repositories
{
    public class StaticPageRepository : IStaticPageRepository
    {
        private readonly ApplicationDbContext _context;

        public StaticPageRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<StaticPage>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.StaticPages
                .AsNoTracking()
                .OrderBy(x => x.Type)
                .ToListAsync(ct);
        }

        public async Task<StaticPage?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _context.StaticPages
                .FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        public async Task<StaticPage?> GetByTypeAsync(PageType type, CancellationToken ct = default)
        {
            return await _context.StaticPages
                .FirstOrDefaultAsync(x => x.Type == type, ct);
        }

        public async Task AddAsync(StaticPage entity, CancellationToken ct = default)
        {
            await _context.StaticPages.AddAsync(entity, ct);
        }

        public void Update(StaticPage entity)
        {
            _context.StaticPages.Update(entity);
        }

        public void Remove(StaticPage entity)
        {
            _context.StaticPages.Remove(entity);
        }
    }
}