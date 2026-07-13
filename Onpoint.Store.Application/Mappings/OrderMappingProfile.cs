using AutoMapper;
using Onpoint.Store.Application.DTOs.Order;
using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Application.Mappings
{
    public class OrderMappingProfile : Profile
    {
        public OrderMappingProfile()
        {
            CreateMap<Order, OrderDto>()
    .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.OrderItems));
            CreateMap<OrderItem, OrderItemDto>();
        }
    }
}
