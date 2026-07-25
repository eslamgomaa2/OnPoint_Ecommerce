namespace Onpoint.Store.Application.DTOs.Branch
{
    public class CreateBranchDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? GoogleMapLocation { get; set; }
        public string? WorkingHours { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDefault { get; set; } = false;
    }
}
