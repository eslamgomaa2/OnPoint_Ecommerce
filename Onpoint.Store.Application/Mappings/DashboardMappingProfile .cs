using AutoMapper;
using Onpoint.Store.Application.DTOs.DashBoard;
using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Application.Mapping
{
    public class DashboardMappingProfile : Profile
    {
        public DashboardMappingProfile()
        {
            CreateMap<Order, LatestOrderDto>()
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Cost, opt => opt.MapFrom(src => src.TotalAmount))
                .ForMember(dest => dest.OrderStatus, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.OrderStatusName, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.OrderDate, opt => opt.MapFrom(src => src.CreatedAt));
        }
    }
}