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

            builder.Property(u => u.FirstName)
                .HasMaxLength(100);

            builder.Property(u => u.LastName)
                .HasMaxLength(100);

            builder.Property(u => u.CreatedAt)
                .IsRequired();

            builder.Property(u => u.IsActive)
                .HasDefaultValue(true);

            builder.Property(u => u.IsDeleted)
                .HasDefaultValue(false);

            builder.Property(u => u.LockoutEscalationLevel)
                .HasDefaultValue(0);

            builder.Property(u => u.BranchRole)
                .HasConversion<string>()
                .HasMaxLength(30);

            builder.HasQueryFilter(u => !u.IsDeleted);

            builder.HasOne(u => u.Branch)
                .WithMany()
                .HasForeignKey(u => u.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(u => u.Cart)
                .WithOne(o => o.User)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(u => u.RefreshTokens)
                .WithOne(o => o.ApplicationUser)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.Addresses)
                .WithOne(o => o.User)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.ToTable("Customers");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.FName).HasMaxLength(100);
            builder.Property(c => c.LName).HasMaxLength(100);
            builder.Property(c => c.Phone).HasMaxLength(20);
            builder.Property(c => c.Email).HasMaxLength(256);
            builder.Property(c => c.Address).HasMaxLength(500);
            builder.Property(c => c.Note).HasMaxLength(1000);

            builder.HasIndex(c => c.Phone);
            builder.HasIndex(c => c.Email);

            builder.HasOne(c => c.Branch)
                .WithMany()
                .HasForeignKey(c => c.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(c => c.Orders)
                .WithOne(o => o.Customer)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

}