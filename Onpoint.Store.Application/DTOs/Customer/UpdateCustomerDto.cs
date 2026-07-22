
namespace Onpoint.Store.Application.DTOs.Customer
{
    public class UpdateCustomerDto
    {
        public string FName { get; set; } = string.Empty;
        public string? LName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }
    }
}