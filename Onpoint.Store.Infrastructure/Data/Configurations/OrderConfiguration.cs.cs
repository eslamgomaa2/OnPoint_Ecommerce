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
            builder.ToTable("Orders");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.InvoiceNumber)
                .HasMaxLength(50);

            builder.HasIndex(o => o.InvoiceNumber)
                .IsUnique()
                .HasFilter("[InvoiceNumber] IS NOT NULL");

            builder.Property(o => o.PhoneNumber)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(o => o.CouponCode)
                .HasMaxLength(50);

            builder.Property(o => o.Note)
                .HasMaxLength(1000);

            builder.Property(o => o.QRCode)
                .HasMaxLength(500);

            builder.Property(o => o.SubTotal).HasColumnType("decimal(18,2)");
            builder.Property(o => o.TaxAmount).HasColumnType("decimal(18,2)");
            builder.Property(o => o.ShippingCost).HasColumnType("decimal(18,2)");
            builder.Property(o => o.DiscountAmount).HasColumnType("decimal(18,2)");
            builder.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");
            builder.Property(o => o.AmountReceived).HasColumnType("decimal(18,2)");
            builder.Property(o => o.Change).HasColumnType("decimal(18,2)");

            builder.Property(o => o.Status)
                .HasConversion<string>()
                .HasMaxLength(30);

            builder.Property(o => o.PaymentMethod)
                .HasConversion<string>()
                .HasMaxLength(30);

            builder.Property(o => o.Source)
                .HasConversion<string>()
                .HasMaxLength(30);

            // Relationships
            builder.HasOne(o => o.Cashier)
                .WithMany()
                .HasForeignKey(o => o.CashierId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(o => o.Branch)
                .WithMany(o => o.Orders)
                .HasForeignKey(o => o.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(o => o.Customer)
     .WithMany(c => c.Orders)
     .HasForeignKey(o => o.CustomerId)
     .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(o => o.User)
    .WithMany()
    .HasForeignKey(o => o.UserId)
    .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(o => o.ShippingAddress)
                .WithMany()
                .HasForeignKey(o => o.AddressId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(o => o.StatusHistory)
                .WithOne(sh => sh.Order)
                .HasForeignKey(sh => sh.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(o => o.Refunds)
                .WithOne(r => r.Order)
                .HasForeignKey(r => r.OrderId)
                .OnDelete(DeleteBehavior.Restrict);


            builder.HasOne(o => o.Invoice)
                .WithOne()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(o => o.Transaction)
                .WithOne()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(oi => oi.Id);

        builder.Property(oi => oi.ProductName)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(oi => oi.ProductImageUrl)
            .HasMaxLength(500);

        builder.Property(oi => oi.VariantDescription)
            .HasMaxLength(250);

        builder.Property(oi => oi.UnitPrice)
            .HasColumnType("decimal(18,2)");

        // Computed / not-mapped columns
        builder.Ignore(oi => oi.NetQuantity);
        builder.Ignore(oi => oi.NetTotalPrice);
        builder.Ignore(oi => oi.TotalPrice);



        builder.HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(oi => oi.Product)
            .WithMany()
            .HasForeignKey(oi => oi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(oi => oi.ProductVariant)
            .WithMany()
            .HasForeignKey(oi => oi.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.ToTable("OrderStatusHistories");

        builder.HasKey(sh => sh.Id);

        builder.Property(sh => sh.Status)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(sh => sh.EventName)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(sh => sh.Description)
            .HasMaxLength(1000);

        builder.Property(sh => sh.EventTime)
            .IsRequired();

        builder.HasOne(sh => sh.Order)
            .WithMany(o => o.StatusHistory)
            .HasForeignKey(sh => sh.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class RefundConfiguration : IEntityTypeConfiguration<Refund>
{
    public void Configure(EntityTypeBuilder<Refund> builder)
    {
        builder.ToTable("Refunds");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Type)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(r => r.TotalAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(r => r.Reason)
            .HasMaxLength(500);

        builder.Property(r => r.RefundedAt)
            .IsRequired();

        builder.HasOne(r => r.Order)
            .WithMany(o => o.Refunds)
            .HasForeignKey(r => r.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Branch)
            .WithMany()
            .HasForeignKey(r => r.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.RefundItems)
            .WithOne(ri => ri.Refund)
            .HasForeignKey(ri => ri.RefundId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class RefundItemConfiguration : IEntityTypeConfiguration<RefundItem>
{
    public void Configure(EntityTypeBuilder<RefundItem> builder)
    {
        builder.ToTable("RefundItems");

        builder.HasKey(ri => ri.Id);

        builder.Property(ri => ri.UnitPrice)
            .HasColumnType("decimal(18,2)");
        builder.Ignore(ri => ri.TotalAmount);


        builder.HasOne(ri => ri.Refund)
            .WithMany(r => r.RefundItems)
            .HasForeignKey(ri => ri.RefundId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ri => ri.OrderItem)
            .WithMany()
            .HasForeignKey(ri => ri.OrderItemId)
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
