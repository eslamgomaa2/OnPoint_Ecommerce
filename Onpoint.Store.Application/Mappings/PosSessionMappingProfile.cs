using AutoMapper;
using Onpoint.Store.Application.DTOs.PosSession;
using Onpoint.Store.Domin.Entities.Sales;

namespace Onpoint.Store.Application.Mappings
{
    public class PosSessionMappingProfile : Profile
    {
        public PosSessionMappingProfile()
        {
            CreateMap<PosSessionItem, PosSessionItemDto>();

            CreateMap<PosSession, PosSessionDto>()
                .ForMember(dest => dest.CashierName, opt => opt.MapFrom(src => src.Cashier != null ? src.Cashier.UserName : string.Empty))
                .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty))
                .ForMember(dest => dest.SubTotal, opt => opt.MapFrom(src => src.Items.Sum(i => i.UnitPrice * i.Quantity)))
                .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.Items.Sum(i => i.UnitPrice * i.Quantity) - src.DiscountAmount));
        }
    }
}