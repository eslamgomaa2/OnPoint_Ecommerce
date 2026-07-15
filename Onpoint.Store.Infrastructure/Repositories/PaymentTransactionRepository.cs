using Microsoft.EntityFrameworkCore;
using Onpoint.Store.Domin.Entities.Sales;
using Onpoint.Store.Domin.Repositories;
using Onpoint.Store.Infrastructure.Data.Context;

namespace Onpoint.Store.Infrastructure.Repositories
{
    public class PaymentTransactionRepository : GenericRepository<PaymentTransaction, int>, IPaymentTransactionRepository
    {
        public PaymentTransactionRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<PaymentTransaction?> GetByGatewayTransactionIdAsync(string gatewayId)
        {
            return await _dbset.FirstOrDefaultAsync(p => p.GatewayTransactionId == gatewayId);
        }

        public async Task<PaymentTransaction?> GetByOrderIdAsync(int orderId)
        {
            return await _dbset
                .Where(t => t.OrderId == orderId)
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefaultAsync();
        }
    }
}