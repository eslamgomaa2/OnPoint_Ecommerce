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

            // ⚠️ Product -> ProductDto
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty))
                .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand != null ? src.Brand.Name : null))
                .ForMember(dest => dest.PrimaryImageUrl, opt => opt.MapFrom(src =>
                    src.Images != null && src.Images.Any(i => i.IsPrimary)
                        ? src.Images.First(i => i.IsPrimary).ImageUrl
                        : null))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))



                .ForMember(dest => dest.Price, opt => opt.MapFrom(src =>
                    src.Variants != null && src.Variants.Any(v => v.IsActive)
                        ? src.Variants.Where(v => v.IsActive).Min(v => v.Price) // or Average, or First
                        : 0))

                .ForMember(dest => dest.DiscountedPrice, opt => opt.MapFrom(src =>
                    src.Variants != null && src.Variants.Any(v => v.IsActive) &&
                    src.Discounts != null && src.Discounts.Any(d => d.IsActive && d.EndDate >= DateTime.UtcNow)
                        ? src.Variants.Where(v => v.IsActive).Min(v => v.Price) -
                          (src.Variants.Where(v => v.IsActive).Min(v => v.Price) *
                           src.Discounts.First(d => d.IsActive && d.EndDate >= DateTime.UtcNow).DiscountPercentage / 100)
                        : (decimal?)null))

                // ⚠️ Total Stock: Only from variants (no product-level stocks)
                .ForMember(dest => dest.TotalStock, opt => opt.MapFrom(src =>
                    src.Variants != null
                        ? src.Variants.Where(v => v.IsActive).SelectMany(v => v.Stocks ?? Enumerable.Empty<Stock>()).Sum(s => s.Quantity)
                        : 0))

                // ⚠️ Stock Status: Only from variants
                .ForMember(dest => dest.StockStatus, opt => opt.MapFrom(src =>
                    src.Variants != null && src.Variants.Where(v => v.IsActive).SelectMany(v => v.Stocks ?? Enumerable.Empty<Stock>()).Sum(s => s.Quantity) > 0
                        ? "InStock"
                        : "OutOfStock"));

            // ⚠️ Product -> ProductDetailDto
            CreateMap<Product, ProductDetailDto>()
                .IncludeBase<Product, ProductDto>()
                // ⚠️ REMOVED: .ForMember(dest => dest.Attributes, opt => opt.MapFrom(src => src.AttributeValues))
                // Product no longer has AttributeValues
                .ForMember(dest => dest.Variants, opt => opt.MapFrom(src => src.Variants != null ? src.Variants.Where(v => v.IsActive) : null))
                .ForMember(dest => dest.BranchStock, opt => opt.Ignore())
                .ForMember(dest => dest.Shipping, opt => opt.MapFrom(src => src.Shipping))
                .ForMember(dest => dest.Translations, opt => opt.MapFrom(src => src.Translations));


            CreateMap<ProductImage, ProductImageDto>();
            CreateMap<Discount, DiscountDto>();

            // ⚠️ ProductVariant -> ProductVariantDto
            CreateMap<ProductVariant, ProductVariantDto>()
               .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
               .ForMember(dest => dest.Attributes, opt => opt.MapFrom(src => src.AttributeValues));

            CreateMap<VariantAttributeValue, VariantAttributeValueDto>()
                .ForMember(dest => dest.ProductAttributeId, opt => opt.MapFrom(src => src.ProductAttributeId))
                .ForMember(dest => dest.AttributeName, opt => opt.MapFrom(src => src.ProductAttribute != null ? src.ProductAttribute.Name : string.Empty))
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value));

            CreateMap<Stock, VariantStockDto>()
                .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty))
                .ForMember(dest => dest.AvailableQuantity, opt => opt.MapFrom(src => src.Quantity - src.ReservedQuantity));

            CreateMap<ProductShipping, ProductShippingDto>();
            CreateMap<ProductTranslation, ProductTranslationDto>();

            /* // ⚠️ NEW: ProductReview -> ProductReviewDto
             CreateMap<ProductReview, ProductReviewDto>()
                 .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : "Unknown"))
                 .ForMember(dest => dest.UserAvatar, opt => opt.MapFrom(src => src.User != null ? src.User.AvatarUrl : null));*/

            // =========================================================
            // 2. Branch Manager Write Mappings (DTO -> Entity)
            // =========================================================
            CreateMap<CreateProductByBranchManagerDto, Product>()
                // ⚠️ REMOVED: .ForMember(dest => dest.AttributeValues, opt => opt.MapFrom(src => src.Attributes))
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
                // ⚠️ REMOVED: No more Price, Cost, Sku, Barcode, QrCode on Product
                .ForMember(dest => dest.Slug, opt => opt.Ignore())
               .ForMember(dest => dest.Discounts, opt => opt.MapFrom(src =>
                    src.Discount != null ? new List<CreateDiscountDto> { src.Discount } : null));

            CreateMap<CreateProductVariantDto, ProductVariant>()
                .ForMember(dest => dest.AttributeValues, opt => opt.MapFrom(src => src.Attributes))
                .ForMember(dest => dest.Stocks, opt => opt.MapFrom(src => src.BranchStocks));

            CreateMap<VariantBranchStockDto, Stock>();
            CreateMap<CreateDiscountDto, Discount>();

            // ⚠️ Update Mappings
            CreateMap<UpdateProductDto, Product>()
                .ForMember(dest => dest.Slug, opt => opt.Ignore());

            CreateMap<UpdateProductVariantDto, ProductVariant>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()) // Handle manually
                .ForMember(dest => dest.ProductId, opt => opt.Ignore());


            CreateMap<VariantAttributeValueDto, VariantAttributeValue>();
            CreateMap<CreateDiscountDto, Discount>();
            CreateMap<CreateProductImageDto, ProductImage>();
            CreateMap<CreateProductShippingDto, ProductShipping>();
            CreateMap<CreateProductTranslationDto, ProductTranslation>();
        }
    }
}