namespace Onpoint.Store.Application.DTOs.Branch
{
    public class CreateBranchDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDefault { get; set; } = false;
    }
}
