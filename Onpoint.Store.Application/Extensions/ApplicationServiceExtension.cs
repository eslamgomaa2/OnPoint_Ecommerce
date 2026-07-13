using BuildingBlocks.Common.Helpers;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Onpoint.Store.Application.Mappings;
using Onpoint.Store.Application.Services.AddressServ;
using Onpoint.Store.Application.Services.AuthServices;
using Onpoint.Store.Application.Services.AuthServices.Email;
using Onpoint.Store.Application.Services.AuthServices.Otp;
using Onpoint.Store.Application.Services.AuthServices.Token;
using Onpoint.Store.Application.Services.CartServ;
using Onpoint.Store.Application.Services.CategoryServ;
using Onpoint.Store.Application.Services.CouponServ;
using Onpoint.Store.Application.Services.InvoiceServ;
using Onpoint.Store.Application.Services.MedioServices;
using Onpoint.Store.Application.Services.OrderServ;
using Onpoint.Store.Application.Services.PaymentTransactionServ;
using Onpoint.Store.Application.Services.ProductServ;
using Onpoint.Store.Application.Services.ReviewServ;
using Onpoint.Store.Application.Services.WishlistServ;
using System.Reflection;

namespace Onpoint.Store.Application.Extensions
{
    public static class ApplicationServiceExtension
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICartService, CartService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IOtpService, OtpService>();
            services.AddScoped<IAddressService, AddressService>();
            services.AddScoped<ICouponService, CouponService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IPaymentTransactionService, PaymentTransactionService>();
            services.AddScoped<IInvoiceService, InvoiceService>();
            services.AddScoped<IWishlistService, WishlistService>();
            services.AddScoped<IReviewService, ReviewService>();

            services.Configure<MailSettings>(configuration.GetSection("MailSettings"));
            services.AddScoped<IMediaService, MediaService>();
            services.AddAutoMapper(typeof(CategoryMappingProfile).Assembly);
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            return services;
        }
    }
}