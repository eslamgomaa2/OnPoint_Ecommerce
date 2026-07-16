using Microsoft.AspNetCore.Identity;
using Onpoint.Store.Domin.Entities.Sales.Onpoint.Store.Domin.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace Onpoint.Store.Domin.Entities
{
    public class ApplicationUser : IdentityUser<int>
    {

        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public int LockoutEscalationLevel { get; set; } = 0;

        [ForeignKey("Branch")]
        public int? BranchId { get; set; }

        public virtual Cart? Cart { get; set; }
        public virtual Branch? Branch { get; set; }
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();
    }
}