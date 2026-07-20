using AutoMapper;
using Onpoint.Store.Application.DTOs.Cart;
using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Application.Mappings
{
    public class CartMappingProfile : Profile
    {
        public CartMappingProfile()
        {
            CreateMap<Cart, CartDto>();

            CreateMap<CartItem, CartItemDto>()
             .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product!.Name))
               .ForMember(dest => dest.ProductImageUrl, opt => opt.MapFrom(src =>
                  src.Product!.Images.FirstOrDefault(i => i.IsPrimary) != null
                   ? src.Product.Images.First(i => i.IsPrimary).ImageUrl
                     : null))
     .ForMember(dest => dest.VariantDescription, opt => opt.MapFrom(src =>
         src.ProductVariant != null
             ? string.Join(", ", src.ProductVariant.AttributeValues.Select(av => av.Value))
             : null));
        }
    }
}