
using Application.Interfaces;
using Application.Validators;
using AutoMapper;
using BuildingBlocks.Results;
using Onpoint.Store.Application.DTOs.Notification;
using Onpoint.Store.Domin.Entities.CustomerEngagement;
using Onpoint.Store.Domin.Repositories;

namespace Application.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork unitOfWork;
    private readonly ServiceResultHandler _resultHandler;
    private readonly IMapper _mapper;

    public NotificationService(
        IUnitOfWork unitOfWork,
        ServiceResultHandler resultHandler,
        IMapper mapper)
    {
        this.unitOfWork = unitOfWork;
        _resultHandler = resultHandler;
        _mapper = mapper;
    }

    public async Task<ServiceResult<PagedResult<NotificationDto>>> GetUserNotificationsAsync(
     int userId,
     NotificationFilterRequest request)
    {
        var (items, totalCount) = await unitOfWork.Notifications.GetByUserIdAsync(
            userId: userId,
            pageNumber: request.PageNumber,
            pageSize: request.PageSize,
            searchTerm: request.SearchTerm,
            isRead: request.IsRead);

        var dtoItems = _mapper.Map<IReadOnlyList<NotificationDto>>(items);

        var pagedResult = PagedResult<NotificationDto>.Create(
            dtoItems,
            totalCount,
            request.PageNumber,
            request.PageSize);

        return _resultHandler.Success(pagedResult);
    }


    public async Task<ServiceResult<NotificationDto>> GetByIdAsync(int id)
    {
        var notification = await unitOfWork.Notifications.GetByIdAsync(id);

        if (notification == null)
            return _resultHandler.NotFound<NotificationDto>("Notification not found");

        var dto = _mapper.Map<NotificationDto>(notification);
        return _resultHandler.Success(dto);
    }

    public async Task<ServiceResult<NotificationDto>> CreateAsync(CreateNotificationDto dto)
    {
        var validator = new CreateNotificationValidator();
        var validation = await validator.ValidateAsync(dto);

        if (!validation.IsValid)
        {
            var errors = validation.Errors.Select(e => e.ErrorMessage).ToList();
            return _resultHandler.BadRequest<NotificationDto>("Validation failed", errors);
        }

        var entity = _mapper.Map<Notification>(dto);
        var created = await unitOfWork.Notifications.CreateAsync(entity);
        var resultDto = _mapper.Map<NotificationDto>(created);

        return _resultHandler.Created(resultDto);
    }

    public async Task<ServiceResult<object>> MarkAsReadAsync(int notificationId, int userId)
    {
        var notification = await unitOfWork.Notifications.GetByIdAsync(notificationId);

        if (notification == null)
            return _resultHandler.NotFound<object>("Notification not found");

        if (notification.UserId != userId)
            return _resultHandler.Forbidden<object>("Access denied");

        notification.IsRead = true;
        await unitOfWork.Notifications.UpdateAsync(notification);

        return _resultHandler.Success("Notification marked as read");
    }

    public async Task<ServiceResult<object>> DeleteAsync(int id)
    {
        var notification = await unitOfWork.Notifications.GetByIdAsync(id);

        if (notification == null)
            return _resultHandler.NotFound<object>("Notification not found");

        await unitOfWork.Notifications.DeleteAsync(id);

        return _resultHandler.Success("Notification deleted successfully");
    }
}