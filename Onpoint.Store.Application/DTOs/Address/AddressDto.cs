namespace Onpoint.Store.Application.DTOs.Address
{
    public class AddressDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty; // "المنزل"، "العمل"
        public string Governorate { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
        public string Block { get; set; } = string.Empty;
        public string? Street { get; set; }
        public string? Avenue { get; set; }
        public string HouseNumber { get; set; } = string.Empty;
        public string? Floor { get; set; }
        public string? Apartment { get; set; }
        public string? ExtraDirections { get; set; }
        public bool IsDefault { get; set; }
    }
}
