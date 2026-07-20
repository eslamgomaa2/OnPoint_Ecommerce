using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Entities.Sales.Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Infrastructure.Data.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.Property(p => p.Price).HasColumnType("decimal(18,2)");

            builder.HasIndex(p => p.Slug).IsUnique();
            builder.HasIndex(p => p.Sku).IsUnique();
            builder.HasIndex(p => p.Barcode).IsUnique().HasFilter("[Barcode] IS NOT NULL");

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

            builder.HasMany(p => p.AttributeValues)
                   .WithOne(av => av.Product)
                   .HasForeignKey(av => av.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
    {
        public void Configure(EntityTypeBuilder<ProductVariant> builder)
        {
            builder.Property(v => v.Price).HasColumnType("decimal(18,2)");
            builder.HasIndex(v => v.Sku).IsUnique();
            builder.HasIndex(v => v.Barcode).IsUnique().HasFilter("[Barcode] IS NOT NULL");

            builder.HasMany(v => v.AttributeValues)
                   .WithOne(av => av.ProductVariant)
                   .HasForeignKey(av => av.ProductVariantId)
                   .OnDelete(DeleteBehavior.Cascade);
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
            builder.HasIndex(b => b.IsDefault)
                   .IsUnique()
                   .HasFilter("[IsDefault] = 1");

        }
    }
    public class StockConfiguration : IEntityTypeConfiguration<Stock>
    {
        public void Configure(EntityTypeBuilder<Stock> builder)
        {
            builder.HasKey(s => s.Id);

            builder.HasOne(s => s.Product)
                   .WithMany(p => p.Stocks)
                   .HasForeignKey(s => s.ProductId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Restrict);

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
}