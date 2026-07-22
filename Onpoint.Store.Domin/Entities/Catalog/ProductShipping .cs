using Onpoint.Store.Domin.Common;
using Onpoint.Store.Domin.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Onpoint.Store.Domin.Entities
{
    public class ProductShipping : BaseEntity
    {
        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        [Column(TypeName = "decimal(18,3)")]
        public decimal? WeightKg { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? LengthCm { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? WidthCm { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? HeightCm { get; set; }

        public bool IsFragile { get; set; }
        public bool IsHazardous { get; set; }

        [MaxLength(50)]
        public ShippingClass? ShippingClass { get; set; }

        [NotMapped]
        public decimal? VolumeCm3 =>
            LengthCm.HasValue && WidthCm.HasValue && HeightCm.HasValue
                ? LengthCm.Value * WidthCm.Value * HeightCm.Value
                : null;
    }
}