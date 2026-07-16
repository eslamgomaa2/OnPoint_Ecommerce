
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Onpoint.Store.Domin.Entities.Sales;

namespace Onpoint.Store.Infrastructure.Data.Configurations
{
    public class PosSessionConfiguration : IEntityTypeConfiguration<PosSession>
    {
        public void Configure(EntityTypeBuilder<PosSession> builder)
        {
            builder.HasOne(s => s.Cashier).WithMany().HasForeignKey(s => s.CashierId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(s => s.Branch).WithMany().HasForeignKey(s => s.BranchId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(s => s.Order).WithOne().HasForeignKey<PosSession>(s => s.OrderId).OnDelete(DeleteBehavior.SetNull);
            builder.HasIndex(s => new { s.CashierId, s.Status });
            builder.HasIndex(s => s.BranchId);
        }
    }

    public class PosSessionItemConfiguration : IEntityTypeConfiguration<PosSessionItem>
    {
        public void Configure(EntityTypeBuilder<PosSessionItem> builder)
        {
            builder.HasOne(i => i.PosSession).WithMany(s => s.Items).HasForeignKey(i => i.PosSessionId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(i => i.Product).WithMany().HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(i => new { i.PosSessionId, i.ProductId }).IsUnique();
        }
    }
}