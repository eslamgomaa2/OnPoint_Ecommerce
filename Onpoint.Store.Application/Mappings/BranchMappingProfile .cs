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
                .ForMember(dest => dest.CashierCount,
                    opt => opt.MapFrom(src => src.Cashiers != null ? src.Cashiers.Count : 0))
                .ForMember(dest => dest.ManagerName,
                    opt => opt.MapFrom(src => src.Manager != null
                        ? $"{src.Manager.FirstName} {src.Manager.LastName}".Trim()
                        : null));


            CreateMap<CreateBranchDto, Branch>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Manager, opt => opt.Ignore())
                .ForMember(dest => dest.Cashiers, opt => opt.Ignore())
                .ForMember(dest => dest.Orders, opt => opt.Ignore());


            CreateMap<UpdateBranchDto, Branch>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Manager, opt => opt.Ignore())
                .ForMember(dest => dest.Cashiers, opt => opt.Ignore())
                .ForMember(dest => dest.Orders, opt => opt.Ignore());
        }
    }
}