using Onpoint.Store.Domin.Repositories;
using Onpoint.Store.Infrastructure.Data.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onpoint.Store.Infrastructure.Repositories
{
    public class UnitOfWork:IUnitOfWork
    {
        
       
            private readonly ApplicationDbContext _context;

            public UnitOfWork(ApplicationDbContext context)
            {
                _context = context;
            }


            public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            {
                return await _context.SaveChangesAsync(cancellationToken);
            }
        
    }
}
