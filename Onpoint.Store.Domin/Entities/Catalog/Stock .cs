using Onpoint.Store.Domin.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Onpoint.Store.Domin.Entities
{
    public class Stock : BaseEntity
    {
        [ForeignKey(nameof(ProductId))]
        public int ProductId { get; set; }
        public virtual Product? Product { get; set; }


        [ForeignKey(nameof(ProductVariantId))]
        public int? ProductVariantId { get; set; }
        public virtual ProductVariant? ProductVariant { get; set; }

        [ForeignKey(nameof(BranchId))]
        public int BranchId { get; set; }
        public virtual Branch? Branch { get; set; }


        public int Quantity { get; set; }
        public int ReservedQuantity { get; set; }
        public int MinimumStockLevel { get; set; } = 0;
        public bool IsLowStock => Quantity <= MinimumStockLevel;
        public int AvailableQuantity => Quantity - ReservedQuantity;
    }
}