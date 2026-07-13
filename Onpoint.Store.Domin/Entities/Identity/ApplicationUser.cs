using Microsoft.AspNetCore.Identity;

namespace Onpoint.Store.Domin.Entities
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public int LockoutEscalationLevel { get; set; } = 0;

        public virtual Cart? Cart { get; set; }

        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public ICollection<Address> Addresses { get; set; } = new List<Address>();
    }
}
