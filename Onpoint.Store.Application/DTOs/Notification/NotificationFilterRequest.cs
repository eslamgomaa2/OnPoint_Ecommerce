using BuildingBlocks.Results;

namespace Onpoint.Store.Application.DTOs.Notification
{
    public class NotificationFilterRequest : PaginationRequest
    {
        public bool? IsRead { get; set; }
    }
}
