using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Domin.Repositories
{
    public interface IUnitOfWork
    {
        ICategoryRepository Categories { get; }
        IProductRepository Products { get; }
        ICartRepository Carts { get; }
        IEmailVerificationOtpRepo EmailVerificationOtpRepo { get; }
        IAddressRepository Addresses { get; }
        ICouponRepository Coupons { get; }
        IOrderRepo Orders { get; }
        IGenericRepository<OrderItem, int> OrderItems { get; }
        IPaymentTransactionRepository PaymentTransactions { get; }
        IInvoiceRepository Invoices { get; }
        IWishlistRepository Wishlists { get; }
        IReviewRepository Reviews { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}