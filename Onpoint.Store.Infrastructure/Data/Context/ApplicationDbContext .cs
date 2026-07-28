using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Onpoint.Store.Domin.Common;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Entities.Identity;
using Onpoint.Store.Domin.Entities.Sales;
using Onpoint.Store.Infrastructure.Data.Configurations;
using System.Reflection;
using static ApplicationUserConfiguration;
namespace Onpoint.Store.Infrastructure.Data.Context
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<StaticPage> StaticPages { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<ProductShipping> ProductShippings { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<EmailVerificationOtp> EmailVerificationOtps { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<ProductTranslation> ProductTranslations { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<PosSession> PosSessions { get; set; }
        public DbSet<PosSessionItem> PosSessionItems { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Refund> Refunds { get; set; }
        public DbSet<RefundItem> RefundItems { get; set; }
        public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }
        public DbSet<AppSettingsInfo> AppSettingsInfo { get; set; }



        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfiguration(new CategoryConfiguration());
            builder.ApplyConfiguration(new ProductConfiguration());
            builder.ApplyConfiguration(new CartConfiguration());
            builder.ApplyConfiguration(new CartItemConfiguration());
            builder.ApplyConfiguration(new OrderConfiguration());
            builder.ApplyConfiguration(new OrderItemConfiguration());
            builder.ApplyConfiguration(new AddressConfiguration());
            builder.ApplyConfiguration(new InvoiceConfiguration());
            builder.ApplyConfiguration(new PaymentTransactionConfiguration());
            builder.ApplyConfiguration(new WishlistConfiguration());
            builder.ApplyConfiguration(new ReviewConfiguration());
            builder.ApplyConfiguration(new CouponConfiguration());
            builder.ApplyConfiguration(new StockConfiguration());
            builder.ApplyConfiguration(new BranchConfiguration());
            builder.ApplyConfiguration(new ApplicationUserConfiguration());
            builder.ApplyConfiguration(new PosSessionConfiguration());
            builder.ApplyConfiguration(new PosSessionItemConfiguration());
            builder.ApplyConfiguration(new DiscountConfiguration());
            builder.ApplyConfiguration(new ProductVariantConfiguration());
            builder.ApplyConfiguration(new VariantAttributeValueConfiguration());
            builder.ApplyConfiguration(new ProductAttributeConfiguration());
            builder.ApplyConfiguration(new ProductAttributeValueConfiguration());
            builder.ApplyConfiguration(new BrandConfiguration());
            builder.ApplyConfiguration(new ProductShippingConfiguration());
            builder.ApplyConfiguration(new ProductTranslationConfiguration());
            builder.ApplyConfiguration(new CustomerConfiguration());
            builder.ApplyConfiguration(new RefundConfiguration());
            builder.ApplyConfiguration(new RefundItemConfiguration());
            builder.ApplyConfiguration(new OrderStatusHistoryConfiguration());
            builder.ApplyConfiguration(new StaticPageConfiguration());
            builder.ApplyConfiguration(new InvoiceConfiguration());


            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    var propertyType = property.ClrType;
                    var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

                    if (underlyingType.IsEnum)
                    {
                        var converterType = typeof(EnumToStringConverter<>).MakeGenericType(underlyingType);
                        var converter = (ValueConverter)Activator.CreateInstance(converterType)!;
                        property.SetValueConverter(converter);
                        property.SetMaxLength(50);
                    }
                }
            }


            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var method = typeof(ApplicationDbContext)
                        .GetMethod(nameof(SetIsDeletedQueryFilter), BindingFlags.NonPublic | BindingFlags.Static)!
                        .MakeGenericMethod(entityType.ClrType);
                    method.Invoke(null, new object[] { builder });
                }
            }

            builder.Entity<ApplicationUser>().HasQueryFilter(u => !u.IsDeleted);
        }

        private static void SetIsDeletedQueryFilter<TEntity>(ModelBuilder builder)
            where TEntity : BaseEntity
        {
            builder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
        }
    }
}