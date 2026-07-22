using Onpoint.Store.Domin.Common;
using System.ComponentModel.DataAnnotations;

namespace Onpoint.Store.Domin.Entities
{
    public class Brand : BaseEntity
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(120)]
        public string Slug { get; set; } = string.Empty;

        public string? LogoUrl { get; set; }


        public int DisplayOrder { get; set; } = 0;

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}