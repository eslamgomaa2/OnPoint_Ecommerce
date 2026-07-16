
using Onpoint.Store.Domin.Entities.Sales;

namespace Onpoint.Store.Domin.Repositories
{
    public interface IPosSessionRepository : IGenericRepository<PosSession, int>
    {
        Task<PosSession?> GetActiveSessionByCashierAsync(int cashierId, CancellationToken ct = default);
        Task<PosSession?> GetSessionWithItemsAsync(int sessionId, CancellationToken ct = default);
    }
}