using AutoMapper;
using Onpoint.Store.Application.DTOs;
using Onpoint.Store.Application.DTOs.Product;
using Onpoint.Store.Application.DTOs.Product.BranchManger;
using Onpoint.Store.Application.DTOs.ProductVariant;
using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Application.Mappings
{
    public class ProductMappingProfile : Profile
    {
        public ProductMappingProfile()
        {
            // =========================================================
            // 1. Read / Query Mappings (Entity -> DTO)
            // =========================================================
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty))
                .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand != null ? src.Brand.Name : null))
                .ForMember(dest => dest.PrimaryImageUrl, opt => opt.MapFrom(src =>
                    src.Images != null && src.Images.Any(i => i.IsPrimary)
                        ? src.Images.First(i => i.IsPrimary).ImageUrl
                        : null))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.FinalPrice, opt => opt.MapFrom(src =>
                    src.Discounts != null && src.Discounts.Any(d => d.IsActive && d.EndDate >= DateTime.UtcNow)
                        ? src.Price - (src.Price * src.Discounts.First(d => d.IsActive && d.EndDate >= DateTime.UtcNow).DiscountPercentage / 100)
                        : src.Price))
                // 🔹 DYNAMICALLY CALCULATE TOTAL STOCK (DIRECT + VARIANT STOCKS)
                .ForMember(dest => dest.TotalStock, opt => opt.MapFrom(src =>
                    (src.Stocks != null ? src.Stocks.Sum(s => s.Quantity) : 0) +
                    (src.Variants != null ? src.Variants.SelectMany(v => v.Stocks ?? Enumerable.Empty<Stock>()).Sum(s => s.Quantity) : 0)
                ))
                // 🔹 DYNAMICALLY CALCULATE STOCK STATUS
                .ForMember(dest => dest.StockStatus, opt => opt.MapFrom(src =>
                    ((src.Stocks != null ? src.Stocks.Sum(s => s.Quantity) : 0) +
                     (src.Variants != null ? src.Variants.SelectMany(v => v.Stocks ?? Enumerable.Empty<Stock>()).Sum(s => s.Quantity) : 0)) > 0
                        ? "InStock"
                        : "OutOfStock"
                ));

            CreateMap<Product, ProductDetailDto>()
                .IncludeBase<Product, ProductDto>()
                .ForMember(dest => dest.Attributes, opt => opt.MapFrom(src => src.AttributeValues))
                .ForMember(dest => dest.Variants, opt => opt.MapFrom(src => src.Variants != null ? src.Variants.Where(v => v.IsActive) : null))
                .ForMember(dest => dest.BranchStock, opt => opt.Ignore())
                .ForMember(dest => dest.Shipping, opt => opt.MapFrom(src => src.Shipping))
                .ForMember(dest => dest.Translations, opt => opt.MapFrom(src => src.Translations));

            CreateMap<ProductAttributeValue, ProductAttributeValueDto>()
               .ForMember(dest => dest.AttributeName, opt => opt.MapFrom(src => src.ProductAttribute != null ? src.ProductAttribute.Name : string.Empty));

            CreateMap<ProductImage, ProductImageDto>();
            CreateMap<Discount, DiscountDto>();

            CreateMap<ProductVariant, ProductVariantDto>()
               .ForMember(dest => dest.Attributes, opt => opt.MapFrom(src => src.AttributeValues))
               .ForMember(dest => dest.Stocks, opt => opt.MapFrom(src => src.Stocks));

            CreateMap<VariantAttributeValue, VariantAttributeValueDto>()
                .ForMember(dest => dest.ProductAttributeId, opt => opt.MapFrom(src => src.ProductAttributeId))
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value));

            CreateMap<Stock, VariantStockDto>()
                .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty))
                .ForMember(dest => dest.AvailableQuantity, opt => opt.MapFrom(src => src.Quantity - src.ReservedQuantity));

            CreateMap<ProductShipping, ProductShippingDto>();
            CreateMap<ProductTranslation, ProductTranslationDto>();

            // =========================================================
            // 2. Branch Manager Write Mappings (DTO -> Entity)
            // =========================================================
            CreateMap<CreateProductByBranchManagerDto, Product>()
                .ForMember(dest => dest.AttributeValues, opt => opt.MapFrom(src => src.Attributes))
                .ForMember(dest => dest.Stocks, opt => opt.MapFrom(src => src.BranchStocks))
                .ForMember(dest => dest.Discounts, opt => opt.MapFrom(src =>
                    src.Discount != null ? new List<CreateDiscountDto> { src.Discount } : null));

            CreateMap<CreateProductVariantByBranchManagerDto, ProductVariant>()
                .ForMember(dest => dest.AttributeValues, opt => opt.MapFrom(src => src.Attributes))
                .ForMember(dest => dest.Stocks, opt => opt.MapFrom(src => src.BranchStocks));

            CreateMap<BranchManagerStockDto, Stock>();

            // =========================================================
            // 3. Admin Write Mappings (DTO -> Entity) 🟢 [تمت الإضافة]
            // =========================================================
            CreateMap<CreateProductDto, Product>()
                .ForMember(dest => dest.AttributeValues, opt => opt.MapFrom(src => src.Attributes))
                .ForMember(dest => dest.Stocks, opt => opt.MapFrom(src => src.BranchStocks))
                .ForMember(dest => dest.Discounts, opt => opt.MapFrom(src =>
                    src.Discount != null ? new List<CreateDiscountDto> { src.Discount } : null));

            CreateMap<CreateProductVariantDto, ProductVariant>()
                .ForMember(dest => dest.AttributeValues, opt => opt.MapFrom(src => src.Attributes))
                .ForMember(dest => dest.Stocks, opt => opt.MapFrom(src => src.BranchStocks));

            CreateMap<VariantBranchStockDto, Stock>();
            CreateMap<CreateDiscountDto, Discount>();

            // Shared Child Mappings
            CreateMap<CreateProductAttributeValueDto, ProductAttributeValue>();
            CreateMap<VariantAttributeValueDto, VariantAttributeValue>();
            CreateMap<CreateDiscountDto, Discount>();
            CreateMap<CreateProductImageDto, ProductImage>();
            CreateMap<CreateProductShippingDto, ProductShipping>();
            CreateMap<CreateProductTranslationDto, ProductTranslation>();
        }
    }
}