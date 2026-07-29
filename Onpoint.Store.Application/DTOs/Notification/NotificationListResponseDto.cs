
using Onpoint.Store.Application.DTOs.Notification;

namespace Application.DTOs.Notification;

public class NotificationListResponseDto
{
    public List<NotificationDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
}