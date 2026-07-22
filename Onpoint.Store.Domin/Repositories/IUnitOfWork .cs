using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Entities.Sales;
using System.Data;

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
        IApplicationUserRepo ApplicationUsers { get; }
        IStockRepository Stocks { get; }
        IBranchRepo Branches { get; }
        IPosSessionRepository PosSessions { get; }
        IProductVariantRepository ProductVariants { get; }
        IGenericRepository<PosSessionItem, int> PosSessionItems { get; }
        IProductAttributeRepository ProductAttributes { get; }
        IDiscountRepo Discounts { get; }
        IBrandRepository Brands { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        Task BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken ct = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}