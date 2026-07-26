using Onpoint.Store.Domin.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Onpoint.Store.Domin.Entities
{
    public class Wishlist : BaseEntity
    {
        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser? User { get; set; }
        [ForeignKey("ProductVariant")]
        public int? ProductVariantId { get; set; }

        public ProductVariant? ProductVariant { get; set; }
        public int ProductId { get; set; }
        [ForeignKey(nameof(ProductId))]
        public virtual Product? Product { get; set; }

    }
}
