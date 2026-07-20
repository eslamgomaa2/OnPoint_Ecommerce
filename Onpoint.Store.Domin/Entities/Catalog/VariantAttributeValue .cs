using Onpoint.Store.Domin.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Onpoint.Store.Domin.Entities
{
    public class VariantAttributeValue : BaseEntity
    {
        [ForeignKey(nameof(ProductVariantId))]
        public int ProductVariantId { get; set; }
        public virtual ProductVariant? ProductVariant { get; set; }

        [ForeignKey(nameof(ProductAttributeId))]
        public int ProductAttributeId { get; set; }
        public virtual ProductAttribute? ProductAttribute { get; set; }

        [Required, MaxLength(300)]
        public string Value { get; set; } = string.Empty;
    }
}