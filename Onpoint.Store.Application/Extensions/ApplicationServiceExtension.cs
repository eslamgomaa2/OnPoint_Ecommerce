using BuildingBlocks.Common.Helpers;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Onpoint.Store.Application.Common;
using Onpoint.Store.Application.Mappings;
using Onpoint.Store.Application.Services.AddressServ;
using Onpoint.Store.Application.Services.AuthServices;
using Onpoint.Store.Application.Services.AuthServices.Email;
using Onpoint.Store.Application.Services.AuthServices.Otp;
using Onpoint.Store.Application.Services.AuthServices.Token;
using Onpoint.Store.Application.Services.BranchServ;
using Onpoint.Store.Application.Services.CartServ;
using Onpoint.Store.Application.Services.CategoryServ;
using Onpoint.Store.Application.Services.CodeGeneration;
using Onpoint.Store.Application.Services.CodeGeneration.BarcodeGeneration;
using Onpoint.Store.Application.Services.CodeGeneration.BarcodeGeneration.Onpoint.Store.Application.Services.CodeGeneration;
using Onpoint.Store.Application.Services.CodeGeneration.QrCodeGeneration;
using Onpoint.Store.Application.Services.CodeGeneration.SkuGeneration;
using Onpoint.Store.Application.Services.CouponServ;
using Onpoint.Store.Application.Services.DiscountServ;
using Onpoint.Store.Application.Services.InvoiceServ;
using Onpoint.Store.Application.Services.MedioServices;
using Onpoint.Store.Application.Services.OrderServ;
using Onpoint.Store.Application.Services.PaymentServ;
using Onpoint.Store.Application.Services.PaymentServices;
using Onpoint.Store.Application.Services.PaymentTransactionServ;
using Onpoint.Store.Application.Services.PosServ;
using Onpoint.Store.Application.Services.ProductAttributeServ;
using Onpoint.Store.Application.Services.ProductServ;
using Onpoint.Store.Application.Services.ProductVariantServ;
using Onpoint.Store.Application.Services.Profile;
using Onpoint.Store.Application.Services.ReviewServ;
using Onpoint.Store.Application.Services.StockServ;
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
            services.AddHttpClient<IMyFatoorahClient, MyFatoorahClient>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IProfileServices, ProfileServices>();
            services.AddScoped<IBranchService, BranchService>();
            services.AddScoped<IPosSessionService, PosSessionService>();
            services.AddScoped<IStockService, StockService>();
            services.AddScoped<IProductVariantService, ProductVariantService>();
            services.AddScoped<IDiscountService, DiscountService>();
            services.AddScoped<ISkuGeneratorService, SkuGeneratorService>();
            services.AddScoped<IQrCodeService, QrCodeService>();
            services.AddScoped<IBarcodeService, BarcodeService>();
            services.AddScoped<IProductAttributeService, ProductAttributeService>();




            services.Configure<MyFatoorahOptions>(configuration.GetSection("MyFatoorah"));

            services.Configure<MailSettings>(configuration.GetSection("MailSettings"));
            services.AddScoped<IMediaService, MediaService>();
            services.AddAutoMapper(typeof(CategoryMappingProfile).Assembly);
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            return services;
        }
    }
}