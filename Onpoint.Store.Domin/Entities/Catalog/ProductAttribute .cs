using Onpoint.Store.Domin.Common;
using Onpoint.Store.Domin.Enums;
using System.ComponentModel.DataAnnotations;

namespace Onpoint.Store.Domin.Entities
{
    public class ProductAttribute : BaseEntity
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? NameEn { get; set; }

        [Required, MaxLength(120)]
        public string Key { get; set; } = string.Empty;

        public AttributeValueType ValueType { get; set; } = AttributeValueType.Text;


        public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

        public virtual ICollection<ProductAttributeValue> ProductValues { get; set; } = new List<ProductAttributeValue>();
        public virtual ICollection<VariantAttributeValue> VariantValues { get; set; } = new List<VariantAttributeValue>();
    }


}