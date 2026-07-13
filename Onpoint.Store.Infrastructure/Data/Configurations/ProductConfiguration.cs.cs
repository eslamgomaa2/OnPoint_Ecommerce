using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Infrastructure.Data.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.Property(p => p.Price).HasColumnType("decimal(18,3)");
            builder.HasIndex(p => p.Slug).IsUnique();

            builder.HasMany(p => p.Images)
                   .WithOne(i => i.Product)
                   .HasForeignKey(i => i.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.Discounts)
                   .WithOne(d => d.Product)
                   .HasForeignKey(d => d.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}