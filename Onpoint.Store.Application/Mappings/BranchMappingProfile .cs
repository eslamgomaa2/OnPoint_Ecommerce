
using AutoMapper;
using Onpoint.Store.Application.DTOs.Branch;
using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Application.Mappings
{
    public class BranchMappingProfile : Profile
    {
        public BranchMappingProfile()
        {
            CreateMap<Branch, BranchDto>()
                .ForMember(dest => dest.CashierCount, opt => opt.MapFrom(src => src.Cashiers != null ? src.Cashiers.Count : 0))
                .ForMember(dest => dest.OrderCount, opt => opt.MapFrom(src => src.Orders != null ? src.Orders.Count : 0));

            CreateMap<CreateBranchDto, Branch>();
            CreateMap<UpdateBranchDto, Branch>();
        }
    }
}