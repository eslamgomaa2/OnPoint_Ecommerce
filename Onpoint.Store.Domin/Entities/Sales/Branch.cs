using Onpoint.Store.Domin.Common;
using Onpoint.Store.Domin.Entities.Identity;
using System.ComponentModel.DataAnnotations.Schema;


namespace Onpoint.Store.Domin.Entities
{
    public class Branch : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? GoogleMapLocation { get; set; }
        public string? WorkingHours { get; set; }
        public bool IsDefault { get; set; } = false;


        public int? ManagerId { get; set; }
        [ForeignKey(nameof(ManagerId))]
        public virtual ApplicationUser? Manager { get; set; }


        public ICollection<ApplicationUser> Cashiers { get; set; } = new List<ApplicationUser>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Customer> Customers { get; set; } = new List<Customer>();

    }
}

