
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Onpoint.Store.Domin.Entities;
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
            builder.HasOne(i => i.PosSession)
                   .WithMany(s => s.Items)
                   .HasForeignKey(i => i.PosSessionId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(i => i.Product)
                   .WithMany()
                   .HasForeignKey(i => i.ProductId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(i => i.ProductVariant)
                   .WithMany()
                   .HasForeignKey(i => i.ProductVariantId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(i => new { i.PosSessionId, i.ProductId, i.ProductVariantId }).IsUnique();
        }
    }

    public class RefundConfiguration : IEntityTypeConfiguration<Refund>
    {
        public void Configure(EntityTypeBuilder<Refund> builder)
        {
            builder.ToTable("Refunds");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Type)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(r => r.TotalAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(r => r.Reason)
                .HasMaxLength(1000);

            builder.Property(r => r.RefundedAt)
                .IsRequired();


            builder.HasIndex(r => r.OrderId);
            builder.HasIndex(r => r.BranchId);
            builder.HasIndex(r => r.ProcessedByUserId);


            builder.HasOne(r => r.Order)
                .WithMany(o => o.Refunds)
                .HasForeignKey(r => r.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Branch)
                .WithMany()
                .HasForeignKey(r => r.BranchId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }


}