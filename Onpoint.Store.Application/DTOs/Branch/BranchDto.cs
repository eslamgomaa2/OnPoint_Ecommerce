namespace Onpoint.Store.Application.DTOs.Branch
{
    public class BranchDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public bool IsDefault { get; set; }
        public int CashierCount { get; set; }
        public int OrderCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
