using AutoMapper;
using Onpoint.Store.Application.DTOs.Coupon;
using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Application.Mappings
{

    public class CouponMappingProfile : Profile
    {
        public CouponMappingProfile()
        {

            CreateMap<CreateCouponDto, Coupon>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UsedCount, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());


            CreateMap<UpdateCouponDto, Coupon>()
                .ForMember(dest => dest.UsedCount, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());


            CreateMap<Coupon, CouponDto>();
        }
    }
}
