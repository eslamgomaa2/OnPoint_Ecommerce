using AutoMapper;
using Onpoint.Store.Application.Common;
using Onpoint.Store.Application.DTOs.Wishlist;
using Onpoint.Store.Domin.Entities;
namespace Onpoint.Store.Application.Mappings
{
    public class WishlistMappingProfile : Profile
    {
        public WishlistMappingProfile()
        {
            CreateMap<Wishlist, WishlistItemDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src =>
                    src.Product != null ? src.Product.Name : "Unknown Product"))
                .ForMember(dest => dest.ProductImageUrl, opt => opt.MapFrom(src =>
                    src.Product != null && src.Product.Images.Any() ? src.Product.Images.First().ImageUrl : null))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src =>
                    src.Product != null ? PricingHelper.CalculateFinalPrice(src.Product) : 0))
                .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src =>
                    src.Product != null
                    && src.Product.IsActive
                    && !src.Product.IsDeleted
                    && src.Product.Stocks.Sum(s => s.Quantity - s.ReservedQuantity) > 0))
                .ForMember(dest => dest.AddedAt, opt => opt.MapFrom(src => src.CreatedAt));
        }
    }
}