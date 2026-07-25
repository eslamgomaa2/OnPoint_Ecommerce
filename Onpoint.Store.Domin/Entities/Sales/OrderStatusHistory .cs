using Onpoint.Store.Domin.Common;
using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Domin.Entities
{
    public class OrderStatusHistory : BaseEntity
    {
        public int OrderId { get; set; }
        public virtual Order Order { get; set; } = null!;

        public OrderStatus? Status { get; set; }
        public EventName EventName { get; set; } = EventName.OrderCreated;
        public string Description { get; set; } = string.Empty;
        public DateTime EventTime { get; set; }
        public int? PerformedByUserId { get; set; }
    }
}