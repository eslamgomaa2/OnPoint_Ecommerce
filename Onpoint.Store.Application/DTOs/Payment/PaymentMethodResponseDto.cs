namespace Onpoint.Store.Application.DTOs.Payment
{
    public class PaymentMethodResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? NameAr { get; set; }
        public string? IconUrl { get; set; }
        public bool IsActive { get; set; }
    }
}
