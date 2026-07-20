using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Infrastructure.Data.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.HasIndex(c => c.Slug).IsUnique();

            builder.HasOne(c => c.ParentCategory)
                   .WithMany(c => c.SubCategories)
                   .HasForeignKey(c => c.ParentCategoryId)
                   .OnDelete(DeleteBehavior.Restrict);
            builder
          .HasMany(c => c.ProductAttributes)
          .WithMany(a => a.Categories)
          .UsingEntity(j => j.ToTable("CategoryProductAttributes"));
        }
    }

}
public class DiscountConfiguration : IEntityTypeConfiguration<Discount>
{
    public void Configure(EntityTypeBuilder<Discount> builder)
    {
        builder.HasOne(d => d.Product)
               .WithMany(p => p.Discounts)
               .HasForeignKey(d => d.ProductId)
               .OnDelete(DeleteBehavior.Cascade);


    }
}
