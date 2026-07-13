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
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src =>
                    src.Product != null ? src.Product.Name : "Unknown Product"))
                .ForMember(dest => dest.ProductImageUrl, opt => opt.MapFrom(src =>
                    src.Product != null && src.Product.Images.Any() ? src.Product.Images.First().ImageUrl : null));
        }
    }
}