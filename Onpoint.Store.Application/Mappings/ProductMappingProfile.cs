using AutoMapper;
using Onpoint.Store.Application.Common;
using Onpoint.Store.Application.DTOs;
using Onpoint.Store.Application.DTOs.Product;
using Onpoint.Store.Application.DTOs.Product.BranchManger;
using Onpoint.Store.Application.DTOs.ProductVariant;
using Onpoint.Store.Application.Mappings.Resolvers;
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

            // ⚠️ Product -> ProductDto
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom<ProductNameResolver>())
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom<ProductCategoryNameResolver>())
                .ForMember(dest => dest.BrandName, opt => opt.MapFrom<ProductBrandNameResolver>())
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src =>
                                src.Images != null
                               ? src.Images.Select(i => new ProductImageDto
                               {
                                   Id = i.Id,
                                   ImageUrl = i.ImageUrl,
                                   IsPrimary = i.IsPrimary
                               }).ToList()
        : new List<ProductImageDto>()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))

                .ForMember(dest => dest.Price, opt => opt.MapFrom(src =>
                    src.Variants != null && src.Variants.Any(v => v.IsActive)
                        ? src.Variants.Where(v => v.IsActive).Min(v => v.Price)
                        : 0))
                .ForMember(dest => dest.Sku, opt => opt.MapFrom(src =>
                          src.Variants != null && src.Variants.Any(v => v.IsActive)
                        ? src.Variants.Where(v => v.IsActive).OrderBy(v => v.Price).First().Sku
                                : null))
                 .ForMember(dest => dest.Cost, opt => opt.MapFrom(src =>
                            src.Variants != null && src.Variants.Any(v => v.IsActive)
                           ? src.Variants.Where(v => v.IsActive).OrderBy(v => v.Price).First().Cost
                           : 0))
                .ForMember(dest => dest.DiscountedPrice, opt => opt.MapFrom(src =>
                    src.Variants != null && src.Variants.Any(v => v.IsActive) &&
                    src.Discounts != null && src.Discounts.Any(d => d.IsActive && d.EndDate >= DateTime.UtcNow)
                        ? src.Variants.Where(v => v.IsActive).Min(v => v.Price) -
                          (src.Variants.Where(v => v.IsActive).Min(v => v.Price) *
                           src.Discounts.First(d => d.IsActive && d.EndDate >= DateTime.UtcNow).DiscountPercentage / 100)
                        : (decimal?)null))

                .ForMember(dest => dest.TotalStock, opt => opt.MapFrom(src =>
                    src.Variants != null
                        ? src.Variants.Where(v => v.IsActive).SelectMany(v => v.Stocks ?? Enumerable.Empty<Stock>()).Sum(s => s.Quantity)
                        : 0))

                .ForMember(dest => dest.StockStatus, opt => opt.MapFrom(src =>
                    src.Variants != null && src.Variants.Where(v => v.IsActive).SelectMany(v => v.Stocks ?? Enumerable.Empty<Stock>()).Sum(s => s.Quantity) > 0
                        ? "InStock"
                        : "OutOfStock"))
                .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src =>
        src.Reviews != null && src.Reviews.Any(r => r.IsApproved)
            ? src.Reviews.Where(r => r.IsApproved).Average(r => r.Rating)
            : 0));


            // ⚠️ Product -> ProductDetailDto
            CreateMap<Product, ProductDetailDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom<ProductDetailNameResolver>())
                .ForMember(dest => dest.Description, opt => opt.MapFrom<ProductDetailDescriptionResolver>())
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom<ProductDetailCategoryNameResolver>())
                .ForMember(dest => dest.BrandName, opt => opt.MapFrom<ProductDetailBrandNameResolver>())
                .ForMember(dest => dest.Variants, opt => opt.MapFrom(src => src.Variants != null ? src.Variants.Where(v => v.IsActive) : null))
                .ForMember(dest => dest.BranchStock, opt => opt.Ignore())
                .ForMember(dest => dest.Shipping, opt => opt.MapFrom(src => src.Shipping));


            CreateMap<ProductImage, ProductImageDto>();
            CreateMap<Discount, DiscountDto>();

            // ⚠️ ProductVariant -> ProductVariantDto
            CreateMap<ProductVariant, ProductVariantDto>()
               .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
               .ForMember(dest => dest.Attributes, opt => opt.MapFrom(src => src.AttributeValues))
               .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => ProductVariantHelper.CalculateAvailableQuantity(src.Stocks, null)))
               .ForMember(dest => dest.InStock, opt => opt.MapFrom(src => ProductVariantHelper.IsInStock(src.Stocks, null)));

            CreateMap<VariantAttributeValue, VariantAttributeValueDto>()
                .ForMember(dest => dest.ProductAttributeId, opt => opt.MapFrom(src => src.ProductAttributeId))
                .ForMember(dest => dest.AttributeName, opt => opt.MapFrom<VariantAttributeNameResolver>())
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value));

            CreateMap<Stock, VariantStockDto>()
                .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty))
                .ForMember(dest => dest.AvailableQuantity, opt => opt.MapFrom(src => src.Quantity - src.ReservedQuantity));

            CreateMap<ProductShipping, ProductShippingDto>();

            // =========================================================
            // 2. Branch Manager Write Mappings (DTO -> Entity)
            // =========================================================
            CreateMap<CreateProductByBranchManagerDto, Product>()
                .ForMember(dest => dest.Discounts, opt => opt.MapFrom(src =>
                    src.Discount != null ? new List<CreateDiscountDto> { src.Discount } : null));

            CreateMap<CreateProductVariantByBranchManagerDto, ProductVariant>()
                .ForMember(dest => dest.AttributeValues, opt => opt.MapFrom(src => src.Attributes))
                .ForMember(dest => dest.Stocks, opt => opt.MapFrom(src => src.BranchStocks));

            CreateMap<BranchManagerStockDto, Stock>();

            // =========================================================
            // 3. Admin Write Mappings (DTO -> Entity)
            // =========================================================
            CreateMap<CreateProductDto, Product>()
                .ForMember(dest => dest.Slug, opt => opt.Ignore())
               .ForMember(dest => dest.Discounts, opt => opt.MapFrom(src =>
                    src.Discount != null ? new List<CreateDiscountDto> { src.Discount } : null));

            CreateMap<CreateProductVariantDto, ProductVariant>()
                .ForMember(dest => dest.AttributeValues, opt => opt.MapFrom(src => src.Attributes))
                .ForMember(dest => dest.Stocks, opt => opt.MapFrom(src => src.BranchStocks));

            CreateMap<VariantBranchStockDto, Stock>();
            CreateMap<CreateDiscountDto, Discount>();

            CreateMap<UpdateProductDto, Product>()
                .ForMember(dest => dest.Slug, opt => opt.Ignore());

            CreateMap<UpdateProductVariantDto, ProductVariant>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ProductId, opt => opt.Ignore());


            CreateMap<VariantAttributeValueDto, VariantAttributeValue>();
            CreateMap<CreateProductImageDto, ProductImage>();
            CreateMap<CreateProductShippingDto, ProductShipping>();
            CreateMap<CreateProductTranslationDto, ProductTranslation>();
        }
    }
}