using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Infrastructure.Data.Configurations
{
    public class WishlistConfiguration : IEntityTypeConfiguration<Wishlist>
    {
        public void Configure(EntityTypeBuilder<Wishlist> builder)
        {
            builder.HasIndex(w => new { w.UserId, w.ProductId }).IsUnique();

            builder.HasOne(w => w.Product)
                   .WithMany()
                   .HasForeignKey(w => w.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.Property(r => r.Rating).IsRequired();
            builder.Property(r => r.Comment).HasMaxLength(1000);

            builder.HasIndex(r => new { r.ProductId, r.IsApproved });
            builder.HasIndex(r => r.UserId);



            builder.HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
    public class StoreSettingsConfiguration : IEntityTypeConfiguration<StoreSettings>
    {
        public void Configure(EntityTypeBuilder<StoreSettings> builder)
        {
            builder.HasIndex(s => s.Key).IsUnique();
            builder.Property(s => s.Key).HasMaxLength(100);
            builder.Property(s => s.Value).HasMaxLength(5000);
        }
    }
    public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
    {
        public void Configure(EntityTypeBuilder<Coupon> builder)
        {
            builder.HasIndex(c => c.Code).IsUnique();
            builder.Property(c => c.Value).HasColumnType("decimal(18,3)");
            builder.Property(c => c.MinOrderAmount).HasColumnType("decimal(18,3)");
        }
    }
    public class ContactMessageConfiguration : IEntityTypeConfiguration<ContactMessage>
    {
        public void Configure(EntityTypeBuilder<ContactMessage> builder)
        {
            builder.ToTable("ContactMessages");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.PhoneNumber)
                .HasMaxLength(30);

            builder.Property(e => e.Message)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(e => e.CreatedAt)
                .IsRequired();

            builder.Property(e => e.IsResolved)
                .IsRequired()
                .HasDefaultValue(false);

            builder.HasIndex(e => e.Email);
        }
    }
    public class StaticPageConfiguration : IEntityTypeConfiguration<StaticPage>
    {
        public void Configure(EntityTypeBuilder<StaticPage> builder)
        {
            builder.ToTable("StaticPages");

            builder.Property(x => x.Title)
                .HasMaxLength(300)
                .IsRequired();

            builder.Property(x => x.Content)
                .HasColumnType("nvarchar(max)")
                .IsRequired();

            builder.Property(x => x.Type)
                .IsRequired();

            builder.HasIndex(x => x.Type).IsUnique();
        }
    }
}