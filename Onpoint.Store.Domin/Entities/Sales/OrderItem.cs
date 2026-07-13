using Onpoint.Store.Domin.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Onpoint.Store.Domin.Entities
{
    public class OrderItem : BaseEntity
    {
        public int OrderId { get; set; }
        [ForeignKey(nameof(OrderId))]
        public virtual Order? Order { get; set; }

        public int ProductId { get; set; }
        [ForeignKey(nameof(ProductId))]
        public virtual Product? Product { get; set; }

        public string ProductName { get; set; } = string.Empty; // بنحفظ اسم المنتج وقت الطلب
        public string? ProductImageUrl { get; set; } = string.Empty; // وصورة المنتج

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; } // السعر وقت الشراء (زي ما عملنا في الكارت)

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice => Quantity * UnitPrice;
    }
}
