using Onpoint.Store.Domin.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Onpoint.Store.Domin.Entities
{
    public class ProductVariant : BaseEntity
    {
        [ForeignKey(nameof(ProductId))]
        public int ProductId { get; set; }
        public virtual Product? Product { get; set; }

        [Required, MaxLength(64)]
        public string Sku { get; set; } = string.Empty;

        [MaxLength(64)]
        public string? Barcode { get; set; }
        public string? BarcodeImagePath { get; set; }

        public string? QrCodeValue { get; set; }
        public string? QrCodeImagePath { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }


        public bool IsActive { get; set; } = true;


        public virtual ICollection<VariantAttributeValue> AttributeValues { get; set; } = new List<VariantAttributeValue>();
        public virtual ICollection<Stock> Stocks { get; set; } = new List<Stock>();
    }
}