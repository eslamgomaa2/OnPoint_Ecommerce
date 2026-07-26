using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Onpoint.Store.Application.Services.AuthServices.ExternalAuthService;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Repositories;
using Onpoint.Store.Infrastructure.Data.Context;
using Onpoint.Store.Infrastructure.ExternalAuthServices;
using Onpoint.Store.Infrastructure.Repositories;

namespace Onpoint.Store.Infrastructure.Extensions
{
    public static class InfrastructureServiceExtension
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            // Register specific repositories
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ICartRepository, CartRepository>();
            services.AddScoped<IEmailVerificationOtpRepo, EmailVerificationOtpRepo>();
            services.AddScoped<IAddressRepository, AddressRepository>();
            services.AddScoped<ICouponRepository, CouponRepository>();
            services.AddScoped<IOrderRepo, OrderRepo>();
            services.AddScoped<IGenericRepository<OrderItem, int>, GenericRepository<OrderItem, int>>();
            services.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();
            services.AddScoped<IPossalesRepository, PosSalesRepository>();
            services.AddScoped<IWishlistRepository, WishlistRepository>();
            services.AddScoped<IReviewRepository, ReviewRepository>();
            services.AddScoped<IApplicationUserRepo, ApplicationUserRepo>();
            services.AddScoped<IBranchRepo, BranchRepo>();
            services.AddScoped<IStockRepository, StockRepository>();
            services.AddScoped<IRefundRepository, RefundRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IBrandRepository, BrandRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IDiscountRepo, DiscountRepo>();
            services.AddScoped<IProductAttributeRepository, ProductAttributeRepository>();
            services.AddScoped<IProductVariantRepository, ProductVariantRepository>();
            services.AddScoped<IContactMessageRepository, ContactMessageRepository>();



            // Register open generic repository
            services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));

            // Register Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register external services
            services.AddSingleton<IImageStorageService, CloudinaryStorageService>();
            services.AddScoped<IExternalAuthService, ExternalAuthService>();

            // Configure ASP.NET Core Identity
            services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromHours(1);
                options.Lockout.AllowedForNewUsers = true;
            })
            .AddRoles<IdentityRole<int>>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

            return services;
        }
    }
}