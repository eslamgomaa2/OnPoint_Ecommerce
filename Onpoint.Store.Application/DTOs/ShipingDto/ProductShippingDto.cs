namespace Onpoint.Store.Application.DTOs.Product
{
    public class ProductShippingDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public decimal? WeightKg { get; set; }
        public decimal? LengthCm { get; set; }
        public decimal? WidthCm { get; set; }
        public decimal? HeightCm { get; set; }
        public bool IsFragile { get; set; }
        public bool IsHazardous { get; set; }
        public string? ShippingClass { get; set; }
        public decimal? VolumeCm3 { get; set; }
    }
}