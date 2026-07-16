using Microsoft.EntityFrameworkCore;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Repositories;
using Onpoint.Store.Infrastructure.Data.Context;
namespace Onpoint.Store.Infrastructure.Repositories
{
    public class StockRepository : GenericRepository<Stock, int>, IStockRepository
    {
        public StockRepository(ApplicationDbContext context) : base(context) { }
        public async Task<Stock?> GetByProductAndBranchAsync(int productId, int branchId, CancellationToken ct = default)
            => await _dbset.FirstOrDefaultAsync(s => s.ProductId == productId && s.BranchId == branchId, ct);
        public async Task<IReadOnlyList<Stock>> GetByProductIdsAsync(IEnumerable<int> productIds, int branchId, CancellationToken ct = default)
            => await _dbset.Where(s => productIds.Contains(s.ProductId) && s.BranchId == branchId).ToListAsync(ct);
    }
}