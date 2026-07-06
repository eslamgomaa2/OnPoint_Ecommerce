using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onpoint.Store.Domin.Repositories
{
    public interface IUnitOfWork
    {

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
