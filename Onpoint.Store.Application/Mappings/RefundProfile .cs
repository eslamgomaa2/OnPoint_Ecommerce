using AutoMapper;
using Onpoint.Store.Application.DTOs.Refund;
using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Application.MappingProfiles
{
    public class RefundProfile : Profile
    {
        public RefundProfile()
        {
            CreateMap<Refund, RefundDto>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.RefundItems));

            CreateMap<RefundItem, RefundItemDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.OrderItem.ProductName));
        }
    }
}