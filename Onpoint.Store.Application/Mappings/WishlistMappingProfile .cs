using AutoMapper;
using Onpoint.Store.Application.Common;
using Onpoint.Store.Application.DTOs.Wishlist;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Enums;

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
                    src.Product != null && src.Product.Images.Any()
                        ? src.Product.Images.First().ImageUrl
                        : null))


                .ForMember(dest => dest.Price, opt => opt.MapFrom(src =>
                    src.Product != null ? PricingHelper.CalculateMinFinalPrice(src.Product) : 0))


                .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src =>
                    src.Product != null
                    && src.Product.Status == ProductStatus.Active
                    && !src.Product.IsDeleted
                    && src.Product.Variants != null
                    && src.Product.Variants.Any(v => v.IsActive
                        && v.Stocks.Sum(s => s.Quantity - s.ReservedQuantity) > 0)))

                .ForMember(dest => dest.AddedAt, opt => opt.MapFrom(src => src.CreatedAt));
        }
    }
}