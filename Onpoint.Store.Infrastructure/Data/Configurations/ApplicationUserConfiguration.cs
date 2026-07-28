using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Entities.Identity;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.FirstName).HasMaxLength(100);
        builder.Property(u => u.LastName).HasMaxLength(100);
        builder.Property(u => u.CreatedAt).IsRequired();
        builder.Property(u => u.IsActive).HasDefaultValue(true);
        builder.Property(u => u.IsDeleted).HasDefaultValue(false);
        builder.Property(u => u.LockoutEscalationLevel).HasDefaultValue(0);
        builder.Property(u => u.BranchRole)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.HasOne(u => u.Branch)
     .WithMany(b => b.Cashiers)
     .HasForeignKey(u => u.BranchId)
     .OnDelete(DeleteBehavior.Restrict);
        // ✅ Cart Relationship - FK في Cart table
        builder.HasOne(u => u.Cart)
            .WithOne(c => c.User)
            .HasForeignKey<Cart>(c => c.UserId)   // ← FK في Cart مش هنا
            .OnDelete(DeleteBehavior.Restrict);

        // ✅ RefreshTokens - FK في RefreshToken table
        builder.HasMany(u => u.RefreshTokens)
            .WithOne(r => r.ApplicationUser)
            .HasForeignKey(r => r.ApplicationUserId)  // ← FK في RefreshToken
            .OnDelete(DeleteBehavior.Cascade);

        // ✅ Addresses - FK في Address table
        builder.HasMany(u => u.Addresses)
            .WithOne(a => a.User)
            .HasForeignKey(a => a.UserId)     // ← FK في Address
            .OnDelete(DeleteBehavior.Cascade);
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

            // ✅ Branch Relationship
            builder.HasOne(c => c.Branch)
                .WithMany(b => b.Customers)       // ← حدد الـ Collection في Branch
                .HasForeignKey(c => c.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            // ✅ Orders - FK في Order table
            builder.HasMany(c => c.Orders)
                .WithOne(o => o.Customer)
                .HasForeignKey(o => o.CustomerId) // ← FK في Order
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}