
using AutoMapper;
using Onpoint.Store.Application.DTOs.Notification;
using Onpoint.Store.Domin.Entities.CustomerEngagement;

namespace Application.Mappings;

public class NotificationProfile : Profile
{
    public NotificationProfile()
    {

        CreateMap<Notification, NotificationDto>()
            .ForMember(dest => dest.CreatedAt,
                opt => opt.MapFrom(src => src.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss")));


        CreateMap<CreateNotificationDto, Notification>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.IsRead, opt => opt.MapFrom(_ => false));
    }
}