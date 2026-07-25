namespace Onpoint.Store.Application.DTOs.Customer
{
    public class CreateCustomerDto
    {
        public string FName { get; set; }
        public string? LName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? Note { get; set; }
    }
}