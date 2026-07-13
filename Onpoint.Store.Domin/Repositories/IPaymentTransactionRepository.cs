using Onpoint.Store.Domin.Entities.Sales;

namespace Onpoint.Store.Domin.Repositories
{
    public interface IPaymentTransactionRepository : IGenericRepository<PaymentTransaction, int>
    {
        Task<PaymentTransaction?> GetByGatewayTransactionIdAsync(string gatewayId);
        Task<PaymentTransaction?> GetByOrderIdAsync(int orderId);
    }
}