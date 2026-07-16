using Onpoint.Store.Domin.Entities.Sales.Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Repositories;
using Onpoint.Store.Infrastructure.Data.Context;

namespace Onpoint.Store.Infrastructure.Repositories
{
    public class BranchRepo : GenericRepository<Branch, int>, IBranchRepo
    {
        public BranchRepo(ApplicationDbContext context) : base(context)
        {
        }
    }
}
