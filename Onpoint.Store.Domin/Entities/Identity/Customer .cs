using Onpoint.Store.Domin.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Onpoint.Store.Domin.Entities.Identity
{
    public class Customer : BaseEntity

    {
        public string? FName { get; set; }
        public string? LName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? Note { get; set; }
        [ForeignKey("Branch")]
        public int? BranchId { get; set; }
        public Branch? Branch { get; set; }
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    }
}
