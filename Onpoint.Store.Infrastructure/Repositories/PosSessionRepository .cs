
using Microsoft.EntityFrameworkCore;
using Onpoint.Store.Domin.Entities.Sales;
using Onpoint.Store.Domin.Enums;
using Onpoint.Store.Domin.Repositories;
using Onpoint.Store.Infrastructure.Data.Context;

namespace Onpoint.Store.Infrastructure.Repositories
{
    public class PosSessionRepository : GenericRepository<PosSession, int>, IPosSessionRepository
    {
        public PosSessionRepository(ApplicationDbContext context) : base(context) { }

        public async Task<PosSession?> GetActiveSessionByCashierAsync(int cashierId, CancellationToken ct = default)
            => await _dbset
                .Include(s => s.Items)
                .Include(s => s.Branch)
                .FirstOrDefaultAsync(s => s.CashierId == cashierId && s.Status == PosSessionStatus.Active, ct);

        public async Task<PosSession?> GetSessionWithItemsAsync(int sessionId, CancellationToken ct = default)
            => await _dbset
                .Include(s => s.Items)
                .Include(s => s.Branch)
                .Include(s => s.Cashier)
                .FirstOrDefaultAsync(s => s.Id == sessionId, ct);
    }
}