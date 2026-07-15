<<<<<<< HEAD
﻿using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Onpoint.Store.Application.Services.AuthServices.ExternalAuthService;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Repositories;
using Onpoint.Store.Infrastructure.Data.Context;
using Onpoint.Store.Infrastructure.ExternalAuthServices;
=======
﻿using Microsoft.Extensions.DependencyInjection;
using Onpoint.Store.Domin.Repositories;
>>>>>>> a4229cd5541012e96d4a2a23d93425d84f1837e3
using Onpoint.Store.Infrastructure.Repositories;

namespace Onpoint.Store.Infrastructure.Extensions
{
<<<<<<< HEAD
    public static class InfrastructureServiceExtension
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {

            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ICartRepository, CartRepository>();
            services.AddScoped<IEmailVerificationOtpRepo, EmailVerificationOtpRepo>();
            services.AddScoped<IAddressRepository, AddressRepository>();
            services.AddScoped<ICouponRepository, CouponRepository>();
            services.AddScoped<IOrderRepo, OrderRepo>();
            services.AddScoped<IGenericRepository<OrderItem, int>, GenericRepository<OrderItem, int>>();
            services.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();
            services.AddScoped<IInvoiceRepository, InvoiceRepository>();
            services.AddScoped<IWishlistRepository, WishlistRepository>();
            services.AddScoped<IReviewRepository, ReviewRepository>();



            services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddSingleton<IImageStorageService, CloudinaryStorageService>();
            services.AddScoped<IExternalAuthService, ExternalAuthService>();

            services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromHours(1);
                options.Lockout.AllowedForNewUsers = true;
            })
                   .AddRoles<IdentityRole<int>>()
                     .AddEntityFrameworkStores<ApplicationDbContext>().AddSignInManager()
                     .AddDefaultTokenProviders();
            return services;


        }
    }
}
=======
    
        public static class InfrastructureServiceExtension
        {
            public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
            {
                
                services.AddScoped<IUnitOfWork, UnitOfWork>();

                return services;
            }
        }
    
}
>>>>>>> a4229cd5541012e96d4a2a23d93425d84f1837e3
