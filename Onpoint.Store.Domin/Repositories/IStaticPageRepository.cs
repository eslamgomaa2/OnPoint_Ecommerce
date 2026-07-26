using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Domin.Repositories
{
    public interface IStaticPageRepository
    {
        Task<IEnumerable<StaticPage>> GetAllAsync(CancellationToken ct = default);
        Task<StaticPage?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<StaticPage?> GetByTypeAsync(PageType type, CancellationToken ct = default);
        Task AddAsync(StaticPage entity, CancellationToken ct = default);
        void Update(StaticPage entity);
        void Remove(StaticPage entity);
    }
}