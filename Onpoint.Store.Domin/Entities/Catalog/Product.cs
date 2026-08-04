using Onpoint.Store.Domin.Common;
using Onpoint.Store.Domin.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Onpoint.Store.Domin.Entities
{
    public class Product : BaseEntity
    {
        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(200)]
        public string? NameEn { get; set; }

        [Required, MaxLength(220)]
        public string Slug { get; set; } = string.Empty;

        public string? Description { get; set; }
        public string? DescriptionEn { get; set; }


        [ForeignKey(nameof(Category))]
        public int CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        [ForeignKey(nameof(BrandId))]
        public int? BrandId { get; set; }
        public virtual Brand? Brand { get; set; }

        public bool IsPopular { get; set; } = false;



        public ProductStatus Status { get; set; } = ProductStatus.Draft;


        public virtual ProductShipping? Shipping { get; set; }
        public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public virtual ICollection<Discount> Discounts { get; set; } = new List<Discount>();


        public virtual ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();


    }
}