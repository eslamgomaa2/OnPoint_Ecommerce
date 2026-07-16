using AutoMapper;
using Onpoint.Store.Application.DTOs.PosSession;
using Onpoint.Store.Domin.Entities;
namespace Onpoint.Store.Application.Mappings
{
    public class PosOrderMappingProfile : Profile
    {
        public PosOrderMappingProfile()
        {
            CreateMap<OrderItem, PosOrderItemDto>();
            CreateMap<Order, PosOrderDto>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.OrderItems))
                .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty))
                .ForMember(dest => dest.CashierName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : string.Empty))
                .ForMember(dest => dest.CustomerPhone, opt => opt.MapFrom(src => src.PhoneNumber));
        }
    }
}