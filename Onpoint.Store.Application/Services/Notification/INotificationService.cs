using BuildingBlocks.Results;
using Onpoint.Store.Application.DTOs.Notification;

namespace Application.Interfaces;

public interface INotificationService
{

    Task<ServiceResult<PagedResult<NotificationDto>>> GetUserNotificationsAsync(int userId, NotificationFilterRequest request);
    Task<ServiceResult<NotificationDto>> GetByIdAsync(int id);
    Task<ServiceResult<NotificationDto>> CreateAsync(CreateNotificationDto dto);
    Task<ServiceResult<object>> MarkAsReadAsync(int notificationId, int userId);
    Task<ServiceResult<object>> DeleteAsync(int id);
}