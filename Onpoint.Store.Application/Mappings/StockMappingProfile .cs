using AutoMapper;
using Onpoint.Store.Application.DTOs.Stock;
using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Application.Mappings
{
    public class StockMappingProfile : Profile
    {
        public StockMappingProfile()
        {
            CreateMap<Stock, StockDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src =>
                    src.Product != null ? src.Product.Name : string.Empty))
                .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src =>
                    src.Branch != null ? src.Branch.Name : string.Empty))
                .ForMember(dest => dest.VariantDescription, opt => opt.MapFrom(src =>
                    src.ProductVariant != null
                        ? string.Join(", ", src.ProductVariant.AttributeValues.Select(av => av.Value))
                        : null))
                .ForMember(dest => dest.AvailableQuantity, opt => opt.MapFrom(src => src.Quantity - src.ReservedQuantity));
        }
    }
}