using Domain.Interfaces;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Entities.Sales;
using Onpoint.Store.Domin.Repositories.Onpoint.Store.Application.Interfaces.Repositories;
using System.Data;

namespace Onpoint.Store.Domin.Repositories
{
    public interface IUnitOfWork
    {
        IShippingRepo Shipping { get; }
        IInvoiceRepo Invoices { get; }
        ICustomerRepository Customers { get; }
        INotificationRepository Notifications { get; }
        IStaticPageRepository StaticPages { get; }
        ICategoryRepository Categories { get; }
        IProductRepository Products { get; }
        ICartRepository Carts { get; }
        IEmailVerificationOtpRepo EmailVerificationOtpRepo { get; }
        IAddressRepository Addresses { get; }
        ICouponRepository Coupons { get; }
        IOrderRepo Orders { get; }
        IRefundRepository Refunds { get; }
        IGenericRepository<OrderItem, int> OrderItems { get; }

        IGenericRepository<VariantAttributeValue, int> VariantAttributes { get; }
        IPaymentTransactionRepository PaymentTransactions { get; }
        IPossalesRepository PosSales { get; }
        IWishlistRepository Wishlists { get; }
        IReviewRepository Reviews { get; }
        IApplicationUserRepo ApplicationUsers { get; }
        IStockRepository Stocks { get; }
        IBranchRepo Branches { get; }
        IPosSessionRepository PosSessions { get; }
        IProductVariantRepository ProductVariants { get; }
        IGenericRepository<PosSessionItem, int> PosSessionItems { get; }
        IGenericRepository<ProductShipping, int> ProductShippings { get; }
        IRefreshTokenRepository RefreshTokens { get; }
        IGenericRepository<RefundItem, int> RefundItems { get; }
        IContactMessageRepository ContactMessages { get; }
        IProductAttributeRepository ProductAttributes { get; }
        IDiscountRepo Discounts { get; }
        IBrandRepository Brands { get; }
        IAppSettingsRepository AppSettings { get; }


        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        Task BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken ct = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}