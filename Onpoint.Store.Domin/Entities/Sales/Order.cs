using Onpoint.Store.Domin.Common;
using Onpoint.Store.Domin.Entities.Sales;
using Onpoint.Store.Domin.Entities.Sales.Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Onpoint.Store.Domin.Entities
{
    public class Order : BaseEntity
    {
        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser? User { get; set; }

        public string OrderNumber { get; set; } = string.Empty;

        public int? AddressId { get; set; }
        [ForeignKey(nameof(AddressId))]
        public virtual Address? ShippingAddress { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;


        public decimal SubTotal { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string? CouponCode { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public PaymentMethod PaymentMethod { get; set; }
        public OrderSource Source { get; set; } = OrderSource.Online;
        [ForeignKey("Branch")]
        public int? BranchId { get; set; }


        public Branch? Branch { get; set; }


        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<PaymentTransaction> Transactions { get; set; } = new List<PaymentTransaction>();
        public virtual Invoice? Invoice { get; set; }
        public virtual PaymentTransaction? Transaction { get; set; }
    }

}
