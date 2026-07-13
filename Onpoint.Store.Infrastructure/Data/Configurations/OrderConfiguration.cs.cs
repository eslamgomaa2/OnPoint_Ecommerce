using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Entities.Sales;

namespace Onpoint.Store.Infrastructure.Data.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {

            builder.Property(o => o.SubTotal).HasColumnType("decimal(18,3)");
            builder.Property(o => o.ShippingCost).HasColumnType("decimal(18,3)");
            builder.Property(o => o.DiscountAmount).HasColumnType("decimal(18,3)");
            builder.Property(o => o.TotalAmount).HasColumnType("decimal(18,3)");

            builder.HasMany(o => o.OrderItems)
                   .WithOne(i => i.Order)
                   .HasForeignKey(i => i.OrderId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(o => o.ShippingAddress)
                    .WithMany(a => a.Orders)
                    .HasForeignKey(o => o.AddressId)
                    .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(o => o.Invoice)
                   .WithOne(i => i.Order)
                   .HasForeignKey<Invoice>(i => i.OrderId)
                   .OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(o => o.Transactions)
                  .WithOne(pt => pt.Order)
                  .HasForeignKey(pt => pt.OrderId)
                  .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.Property(o => o.UnitPrice).HasColumnType("decimal(18,3)");
            builder.Ignore(o => o.TotalPrice); // حسابية مش محفوظة

            builder.HasOne(oi => oi.Product)
                   .WithMany()
                   .HasForeignKey(oi => oi.ProductId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class AddressConfiguration : IEntityTypeConfiguration<Address>
    {
        public void Configure(EntityTypeBuilder<Address> builder)
        {
        }
    }

    public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {
            builder.Property(i => i.SubTotal).HasColumnType("decimal(18,3)");
            builder.Property(i => i.TaxAmount).HasColumnType("decimal(18,3)");
            builder.Property(i => i.TotalAmount).HasColumnType("decimal(18,3)");
        }
    }

    public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
    {
        public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
        {
            builder.Property(p => p.Amount).HasColumnType("decimal(18,3)");
        }
    }
}