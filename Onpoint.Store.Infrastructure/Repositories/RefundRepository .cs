using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Repositories;
using Onpoint.Store.Infrastructure.Data.Context;

namespace Onpoint.Store.Infrastructure.Repositories
{
    public class RefundRepository : GenericRepository<Refund, int>, IRefundRepository
    {
        public RefundRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}