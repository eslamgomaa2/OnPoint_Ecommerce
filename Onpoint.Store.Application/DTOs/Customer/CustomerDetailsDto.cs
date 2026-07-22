
namespace Onpoint.Store.Application.DTOs.Customer
{
    public class CustomerDetailsDto
    {
        public int Id { get; set; }
        public string? FName { get; set; }
        public string? LName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public int OrdersCount { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime? LastPurchase { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}