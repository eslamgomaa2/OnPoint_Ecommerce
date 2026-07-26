using AutoMapper;
using Onpoint.Store.Application.DTOs.Brand;
using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Application.Mapping
{
    public class BrandMappingProfile : Profile
    {
        public BrandMappingProfile()
        {
            CreateMap<Brand, BrandDto>()
                .ForMember(dest => dest.ProductsCount,
                           opt => opt.MapFrom(src => src.Products.Count));

            CreateMap<CreateBrandDto, Brand>()
                .ForMember(dest => dest.Slug, opt => opt.Ignore());

            CreateMap<UpdateBrandDto, Brand>()
                .ForMember(dest => dest.Slug, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore());
        }
    }
}