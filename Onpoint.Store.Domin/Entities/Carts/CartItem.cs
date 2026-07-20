using Onpoint.Store.Domin.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Onpoint.Store.Domin.Entities
{
    public class CartItem : BaseEntity
    {
        public int CartId { get; set; }
        [ForeignKey(nameof(CartId))]
        public virtual Cart? Cart { get; set; }

        public int ProductId { get; set; }
        [ForeignKey(nameof(ProductId))]
        public virtual Product? Product { get; set; }

        public int? ProductVariantId { get; set; }
        [ForeignKey(nameof(ProductVariantId))]
        public virtual ProductVariant? ProductVariant { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }
    }
}