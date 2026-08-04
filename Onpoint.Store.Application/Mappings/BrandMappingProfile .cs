using AutoMapper;
using Onpoint.Store.Application.DTOs.Brand;
using Onpoint.Store.Application.Mapping.Resolvers;
using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Application.Mapping
{
    public class BrandMappingProfile : Profile
    {
        public BrandMappingProfile()
        {
            CreateMap<Brand, BrandDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom<BrandNameResolver>())
                .ForMember(dest => dest.ProductsCount,
                            opt => opt.MapFrom(src => src.Products != null ? src.Products.Count : 0));

            CreateMap<CreateBrandDto, Brand>()
                .ForMember(dest => dest.Slug, opt => opt.Ignore())
                .ForMember(dest => dest.Products, opt => opt.Ignore());

            CreateMap<UpdateBrandDto, Brand>()
                .ForMember(dest => dest.Slug, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Products, opt => opt.Ignore());
        }
    }
}