using Onpoint.Store.Domin.Common;

namespace Onpoint.Store.Domin.Entities.Sales
{
    namespace Onpoint.Store.Domin.Entities
    {
        public class Branch : BaseEntity
        {
            public string Name { get; set; } = string.Empty;
            public string? Address { get; set; }
            public string? PhoneNumber { get; set; }
            public bool IsActive { get; set; } = true;
            public bool IsDefault { get; set; } = false;


            public ICollection<ApplicationUser> Cashiers { get; set; } = new List<ApplicationUser>();
            public ICollection<Order> Orders { get; set; } = new List<Order>();
        }
    }
}
