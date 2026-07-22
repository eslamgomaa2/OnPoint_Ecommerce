using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Entities.Identity;

namespace Onpoint.Store.Infrastructure.Data.Configurations
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.Property(u => u.IsDeleted).HasDefaultValue(false);
            builder.Property(u => u.DeletedAt).IsRequired(false);
            builder.Property(u => u.BranchId).IsRequired(false);
        }
    }
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.Property(c => c.FName)
                   .HasMaxLength(5050);

            builder.Property(c => c.LName)
                   .HasMaxLength(50);

            builder.Property(c => c.Email)
                   .HasMaxLength(256);

            builder.Property(c => c.Phone)
                   .HasMaxLength(30);

            builder.Property(c => c.Address)
                   .HasMaxLength(500);

            builder.HasOne(c => c.Branch)
                   .WithMany(b => b.Customers)
                   .HasForeignKey(c => c.BranchId)
                   .OnDelete(DeleteBehavior.Restrict);

        }
    }
}