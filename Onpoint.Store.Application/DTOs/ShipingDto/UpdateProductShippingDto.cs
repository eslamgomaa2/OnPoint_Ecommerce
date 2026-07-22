using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Application.DTOs.Product
{
    public class UpdateProductShippingDto
    {
        public int? Id { get; set; }
        public decimal? WeightKg { get; set; }
        public decimal? LengthCm { get; set; }
        public decimal? WidthCm { get; set; }
        public decimal? HeightCm { get; set; }
        public bool IsFragile { get; set; }
        public bool IsHazardous { get; set; }
        public ShippingClass? ShippingClass { get; set; }
    }
}