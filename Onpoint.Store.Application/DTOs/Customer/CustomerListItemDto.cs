
namespace Onpoint.Store.Application.DTOs.Customer
{
    public class CustomerListItemDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public int OrdersCount { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime? LastPurchase { get; set; }
        public bool IsActive { get; set; }
    }
}