using Onpoint.Store.Domin.Common;
using Onpoint.Store.Domin.Entities.Identity;
using Onpoint.Store.Domin.Entities.Sales;
using Onpoint.Store.Domin.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Onpoint.Store.Domin.Entities
{
    public class Order : BaseEntity
    {
        public int? CashierId { get; set; }
        [ForeignKey(nameof(CashierId))]
        public virtual ApplicationUser? Cashier { get; set; }
        [ForeignKey("Branch")]
        public int BranchId { get; set; }
        public Branch? Branch { get; set; }
        public int CustomerId { get; set; }
        [ForeignKey(nameof(CustomerId))]
        public virtual Customer? Customer { get; set; }

        public string? InvoiceNumber { get; set; }

        public int? AddressId { get; set; }
        [ForeignKey(nameof(AddressId))]
        public virtual Address? ShippingAddress { get; set; }

        public string PhoneNumber { get; set; } = string.Empty;
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountReceived { get; set; }
        public decimal Change { get; set; }
        public string? CouponCode { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public PaymentMethod PaymentMethod { get; set; }
        public OrderSource Source { get; set; } = OrderSource.Online;
        public string? Note { get; set; }



        public string? QRCode { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<PaymentTransaction> Transactions { get; set; } = new List<PaymentTransaction>();
        public virtual ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
        public virtual ICollection<Refund> Refunds { get; set; } = new List<Refund>();
        public virtual Invoice? Invoice { get; set; }
        public virtual PaymentTransaction? Transaction { get; set; }
    }
}