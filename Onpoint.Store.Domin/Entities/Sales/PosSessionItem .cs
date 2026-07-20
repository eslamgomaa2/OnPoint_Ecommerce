using Onpoint.Store.Domin.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Onpoint.Store.Domin.Entities.Sales
{
    public class PosSessionItem : BaseEntity
    {
        public int PosSessionId { get; set; }
        [ForeignKey(nameof(PosSessionId))]
        public virtual PosSession? PosSession { get; set; }

        public int ProductId { get; set; }
        [ForeignKey(nameof(ProductId))]
        public virtual Product? Product { get; set; }

        public int? ProductVariantId { get; set; }
        [ForeignKey(nameof(ProductVariantId))]
        public virtual ProductVariant? ProductVariant { get; set; }

        public string ProductName { get; set; } = string.Empty;
        public string? VariantDescription { get; set; }
        public string? ProductImageUrl { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
    }
}