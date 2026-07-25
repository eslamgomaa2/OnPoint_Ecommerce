using Microsoft.EntityFrameworkCore;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Repositories;
using Onpoint.Store.Infrastructure.Data.Context;

namespace Onpoint.Store.Infrastructure.Repositories
{
    public class PosSalesRepository : GenericRepository<Invoice, int>, IPossalesRepository
    {
        public PosSalesRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Invoice?> GetByOrderIdAsync(int orderId)
        {
            return await _dbset.FirstOrDefaultAsync(i => i.OrderId == orderId);
        }

        public async Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber)
        {
            return await _dbset.FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber);
        }
    }
}