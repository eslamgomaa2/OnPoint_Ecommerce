using AutoMapper;
using Onpoint.Store.Application.DTOs.ProductAttribute;
using Onpoint.Store.Application.Mappings.Resolvers;
using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Application.Mappings
{
    public class ProductAttributeMappingProfile : Profile
    {
        public ProductAttributeMappingProfile()
        {
            CreateMap<ProductAttribute, ProductAttributeDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom<ProductAttributeNameResolver>());

            CreateMap<Category, CategoryBriefDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom<CategoryBriefNameResolver>());

            CreateMap<CreateProductAttributeDto, ProductAttribute>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Categories, opt => opt.Ignore())
                .ForMember(dest => dest.ProductValues, opt => opt.Ignore())
                .ForMember(dest => dest.VariantValues, opt => opt.Ignore());

            CreateMap<UpdateProductAttributeDto, ProductAttribute>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Categories, opt => opt.Ignore())
                .ForMember(dest => dest.ProductValues, opt => opt.Ignore())
                .ForMember(dest => dest.VariantValues, opt => opt.Ignore());
        }
    }
}