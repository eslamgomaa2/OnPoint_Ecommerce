namespace Onpoint.Store.Application.DTOs.Auth.Profile
{
    public class UserLoginInfoDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }




        public string? BranchName { get; set; }

    }
}