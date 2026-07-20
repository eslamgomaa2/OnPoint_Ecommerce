using Microsoft.EntityFrameworkCore.Storage;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Entities.Sales;
using Onpoint.Store.Domin.Repositories;
using Onpoint.Store.Infrastructure.Data.Context;
using System.Data;

namespace Onpoint.Store.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _currentTransaction;

        private ICategoryRepository? _categories;
        private IProductRepository? _products;
        private ICartRepository? _carts;
        private IEmailVerificationOtpRepo? _emailVerificationOtpRepo;
        private IAddressRepository? _addresses;
        private ICouponRepository? _couponRepository;
        private IOrderRepo? _orders;
        private IPaymentTransactionRepository? _paymentTransactions;
        private IInvoiceRepository? _invoices;
        private IWishlistRepository? _wishlists;
        private IReviewRepository? _reviews;
        private IGenericRepository<OrderItem, int>? _orderItems;
        private IApplicationUserRepo? _applicationUsers;
        private IStockRepository? _stockRepository;
        private IBranchRepo? _branches;
        private IPosSessionRepository? _posSessions;
        private IProductVariantRepository? _productVariantRepository;
        private IProductAttributeRepository? _productAttributes;
        private IDiscountRepo? _discounts;


        private IGenericRepository<PosSessionItem, int>? _posSessionItems;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IPaymentTransactionRepository PaymentTransactions => _paymentTransactions ??= new PaymentTransactionRepository(_context);
        public IInvoiceRepository Invoices => _invoices ??= new InvoiceRepository(_context);
        public ICategoryRepository Categories => _categories ??= new CategoryRepository(_context);
        public IProductRepository Products => _products ??= new ProductRepository(_context);
        public ICartRepository Carts => _carts ??= new CartRepository(_context);
        public IEmailVerificationOtpRepo EmailVerificationOtpRepo => _emailVerificationOtpRepo ??= new EmailVerificationOtpRepo(_context);
        public IAddressRepository Addresses => _addresses ??= new AddressRepository(_context);
        public ICouponRepository Coupons => _couponRepository ??= new CouponRepository(_context);
        public IGenericRepository<OrderItem, int> OrderItems => _orderItems ??= new GenericRepository<OrderItem, int>(_context);
        public IOrderRepo Orders => _orders ??= new OrderRepo(_context);
        public IWishlistRepository Wishlists => _wishlists ??= new WishlistRepository(_context);
        public IReviewRepository Reviews => _reviews ??= new ReviewRepository(_context);
        public IApplicationUserRepo ApplicationUsers => _applicationUsers ??= new ApplicationUserRepo(_context);

        public IProductAttributeRepository ProductAttributes => _productAttributes ??= new ProductAttributeRepository(_context);
        public IStockRepository Stocks => _stockRepository ??= new StockRepository(_context);

        public IBranchRepo Branches => _branches ??= new BranchRepo(_context);
        public IGenericRepository<PosSessionItem, int> PosSessionItems => _posSessionItems ??= new GenericRepository<PosSessionItem, int>(_context);

        public IPosSessionRepository PosSessions => _posSessions ??= new PosSessionRepository(_context);

        public IProductVariantRepository ProductVariants => _productVariantRepository ??= new ProductVariantRepository(_context);

        public IDiscountRepo Discounts => _discounts ??= new DiscountRepo(_context);

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
        {
            _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await _context.SaveChangesAsync(cancellationToken);

                if (_currentTransaction != null)
                {
                    await _currentTransaction.CommitAsync(cancellationToken);
                }
            }
            catch
            {
                await RollbackTransactionAsync(cancellationToken);
                throw;
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.RollbackAsync(cancellationToken);
                }
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }
            }
        }
    }
}