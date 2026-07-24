namespace Onpoint.Store.Application.DTOs.Branch
{
    public class BranchDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? GoogleMapLocation { get; set; }
        public string? WorkingHours { get; set; }
        public bool IsActive { get; set; }
        public bool IsDefault { get; set; }
        public bool IsDeleted { get; set; }

        public int? ManagerId { get; set; }
        public string? ManagerName { get; set; }

        public int CashierCount { get; set; }
    }

}
