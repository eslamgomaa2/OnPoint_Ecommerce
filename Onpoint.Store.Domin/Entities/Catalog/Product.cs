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

        [Required, MaxLength(220)]
        public string Slug { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public int CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        public bool IsPopular { get; set; } = false;


        [Required, MaxLength(64)]
        public string Sku { get; set; } = string.Empty;


        [MaxLength(64)]
        public string? Barcode { get; set; }
        public string? BarcodeImagePath { get; set; }


        public string? QrCodeValue { get; set; }
        public string? QrCodeImagePath { get; set; }


        public ProductStatus Status { get; set; } = ProductStatus.Draft;

        public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public virtual ICollection<Discount> Discounts { get; set; } = new List<Discount>();
        public virtual ICollection<Stock> Stocks { get; set; } = new List<Stock>();

        public virtual ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
        public virtual ICollection<ProductAttributeValue> AttributeValues { get; set; } = new List<ProductAttributeValue>();
    }
}