using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Infrastructure.Data.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {


            builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);

            builder.HasMany(p => p.Images)
                   .WithOne(i => i.Product)
                   .HasForeignKey(i => i.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.Discounts)
                   .WithOne(d => d.Product)
                   .HasForeignKey(d => d.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.Variants)
                   .WithOne(v => v.Product)
                   .HasForeignKey(v => v.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);


            builder.HasOne(p => p.Shipping)
                    .WithOne(s => s.Product)
                    .HasForeignKey<ProductShipping>(s => s.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);



            builder.HasMany(p => p.Reviews)
               .WithOne(r => r.Product)
               .HasForeignKey(r => r.ProductId)
               .OnDelete(DeleteBehavior.Restrict); // زي ما هي في الميجريشن الأصلية (Restrict)

        }
    }


    public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
    {
        public void Configure(EntityTypeBuilder<ProductVariant> builder)
        {
            builder.Property(v => v.Price).HasColumnType("decimal(18,2)");
            builder.HasIndex(v => v.Sku).IsUnique();
            builder.HasIndex(v => v.Barcode).IsUnique().HasFilter("[Barcode] IS NOT NULL");

            // تعريف العلاقة بشكل صريح مع الـ Product لمنع إنشاء ProductId1
            builder.HasOne(v => v.Product)
                   .WithMany(p => p.Variants)
                   .HasForeignKey(v => v.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(v => v.AttributeValues)
                   .WithOne(av => av.ProductVariant)
                   .HasForeignKey(av => av.ProductVariantId)
                   .OnDelete(DeleteBehavior.Cascade);

            // إضافه علاقة الـ Stocks أيضاً لضمان سلامتها
            builder.HasMany(v => v.Stocks)
                   .WithOne(s => s.ProductVariant)
                   .HasForeignKey(s => s.ProductVariantId)
                   .OnDelete(DeleteBehavior.Restrict);


            builder.HasIndex(v => new { v.Id });
        }
    }


    public class ProductAttributeConfiguration : IEntityTypeConfiguration<ProductAttribute>
    {
        public void Configure(EntityTypeBuilder<ProductAttribute> builder)
        {
            builder.HasIndex(a => a.Key).IsUnique();
            builder.Property(a => a.ValueType).HasConversion<string>().HasMaxLength(20);
        }
    }

    public class ProductAttributeValueConfiguration : IEntityTypeConfiguration<ProductAttributeValue>
    {
        public void Configure(EntityTypeBuilder<ProductAttributeValue> builder)
        {
            builder.HasIndex(v => new { v.ProductId, v.ProductAttributeId }).IsUnique();
        }
    }

    public class VariantAttributeValueConfiguration : IEntityTypeConfiguration<VariantAttributeValue>
    {
        public void Configure(EntityTypeBuilder<VariantAttributeValue> builder)
        {
            builder.HasIndex(v => new { v.ProductVariantId, v.ProductAttributeId }).IsUnique();
        }
    }
    public class BranchConfiguration : IEntityTypeConfiguration<Branch>
    {
        public void Configure(EntityTypeBuilder<Branch> builder)
        {
            builder.HasKey(b => b.Id);

            builder.HasOne(b => b.Manager)
                   .WithMany()
                   .HasForeignKey(b => b.ManagerId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(b => b.Cashiers)
                   .WithOne(u => u.Branch)
                   .HasForeignKey(u => u.BranchId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(b => b.IsDefault)
                   .IsUnique()
                   .HasFilter("[IsDefault] = 1");

            builder.HasIndex(b => b.IsActive);
            builder.HasIndex(b => b.IsDeleted);
            builder.HasIndex(b => b.ManagerId);
            builder.HasIndex(b => b.Name);
        }
    }
    public class StockConfiguration : IEntityTypeConfiguration<Stock>
    {
        public void Configure(EntityTypeBuilder<Stock> builder)
        {
            builder.HasKey(s => s.Id);


            builder.HasOne(s => s.Product)
                   .WithMany()
                   .HasForeignKey(s => s.ProductId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(s => s.ProductVariant)
                   .WithMany(v => v.Stocks)
                   .HasForeignKey(s => s.ProductVariantId)
                   .IsRequired(false)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.Branch)
                   .WithMany()
                   .HasForeignKey(s => s.BranchId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(s => new { s.ProductId, s.ProductVariantId, s.BranchId })
                   .IsUnique();
        }
    }
    public class ProductShippingConfiguration : IEntityTypeConfiguration<ProductShipping>
    {
        public void Configure(EntityTypeBuilder<ProductShipping> builder)
        {
            builder.ToTable("ProductShippings");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.WeightKg).HasColumnType("decimal(18,3)");
            builder.Property(x => x.LengthCm).HasColumnType("decimal(18,2)");
            builder.Property(x => x.WidthCm).HasColumnType("decimal(18,2)");
            builder.Property(x => x.HeightCm).HasColumnType("decimal(18,2)");
            builder.Property(x => x.ShippingClass).HasMaxLength(50);

            builder.HasOne(x => x.Product)
                .WithOne(p => p.Shipping)
                .HasForeignKey<ProductShipping>(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }


    }
    public class ProductTranslationConfiguration : IEntityTypeConfiguration<ProductTranslation>
    {
        public void Configure(EntityTypeBuilder<ProductTranslation> builder)
        {
            builder.ToTable("ProductTranslations");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.LanguageCode).HasMaxLength(10).IsRequired();
            builder.Property(x => x.Name).HasMaxLength(200);
            builder.Property(x => x.MetaTitle).HasMaxLength(200);
            builder.Property(x => x.MetaDescription).HasMaxLength(500);

            // Unique: ProductId + LanguageCode
            builder.HasIndex(x => new { x.ProductId, x.LanguageCode }).IsUnique();


        }
    }
}