using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Domin.Repositories
{
    public interface IInvoiceRepository : IGenericRepository<Invoice, int>
    {
        Task<Invoice?> GetByOrderIdAsync(int orderId);
        Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber);
    }
}