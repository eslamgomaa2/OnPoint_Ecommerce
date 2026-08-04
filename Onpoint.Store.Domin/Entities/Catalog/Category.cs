using Onpoint.Store.Domin.Common;
using System.ComponentModel.DataAnnotations;

namespace Onpoint.Store.Domin.Entities
{
    public class Category : BaseEntity
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(100)]
        public string? NameEn { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
        [MaxLength(500)]
        public string? DescriptionEn { get; set; }


        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
        public virtual ICollection<ProductAttribute> ProductAttributes { get; set; } = new List<ProductAttribute>();
    }
}