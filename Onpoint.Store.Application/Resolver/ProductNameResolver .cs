using AutoMapper;
using Onpoint.Store.Application.DTOs.Product;
using Onpoint.Store.Application.Mapping;
using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Application.Mappings.Resolvers
{
    // ---------- ProductDto ----------
    public class ProductNameResolver : IValueResolver<Product, ProductDto, string>
    {
        public string Resolve(Product src, ProductDto dest, string destMember, ResolutionContext context)
        {
            var lang = LocalizationHelper.GetLang(context);
            return LocalizationHelper.Pick(src.Name, src.NameEn, lang);
        }
    }

    public class ProductCategoryNameResolver : IValueResolver<Product, ProductDto, string>
    {
        public string Resolve(Product src, ProductDto dest, string destMember, ResolutionContext context)
        {
            var lang = LocalizationHelper.GetLang(context);
            return src.Category != null ? LocalizationHelper.Pick(src.Category.Name, src.Category.NameEn, lang) : string.Empty;
        }
    }

    public class ProductBrandNameResolver : IValueResolver<Product, ProductDto, string?>
    {
        public string? Resolve(Product src, ProductDto dest, string destMember, ResolutionContext context)
        {
            var lang = LocalizationHelper.GetLang(context);
            return src.Brand != null ? LocalizationHelper.Pick(src.Brand.Name, src.Brand.NameEn, lang) : null;
        }
    }

    // ---------- ProductDetailDto ----------
    public class ProductDetailNameResolver : IValueResolver<Product, ProductDetailDto, string>
    {
        public string Resolve(Product src, ProductDetailDto dest, string destMember, ResolutionContext context)
        {
            var lang = LocalizationHelper.GetLang(context);
            return LocalizationHelper.Pick(src.Name, src.NameEn, lang);
        }
    }

    public class ProductDetailDescriptionResolver : IValueResolver<Product, ProductDetailDto, string?>
    {
        public string? Resolve(Product src, ProductDetailDto dest, string destMember, ResolutionContext context)
        {
            var lang = LocalizationHelper.GetLang(context);
            return LocalizationHelper.PickNullable(src.Description, src.DescriptionEn, lang);
        }
    }

    public class ProductDetailCategoryNameResolver : IValueResolver<Product, ProductDetailDto, string>
    {
        public string Resolve(Product src, ProductDetailDto dest, string destMember, ResolutionContext context)
        {
            var lang = LocalizationHelper.GetLang(context);
            return src.Category != null ? LocalizationHelper.Pick(src.Category.Name, src.Category.NameEn, lang) : string.Empty;
        }
    }

    public class ProductDetailBrandNameResolver : IValueResolver<Product, ProductDetailDto, string?>
    {
        public string? Resolve(Product src, ProductDetailDto dest, string destMember, ResolutionContext context)
        {
            var lang = LocalizationHelper.GetLang(context);
            return src.Brand != null ? LocalizationHelper.Pick(src.Brand.Name, src.Brand.NameEn, lang) : null;
        }
    }

    // ---------- VariantAttributeValueDto ----------
    public class VariantAttributeNameResolver : IValueResolver<VariantAttributeValue, VariantAttributeValueDto, string>
    {
        public string Resolve(VariantAttributeValue src, VariantAttributeValueDto dest, string destMember, ResolutionContext context)
        {
            var lang = LocalizationHelper.GetLang(context);
            return src.ProductAttribute != null
                ? LocalizationHelper.Pick(src.ProductAttribute.Name, src.ProductAttribute.NameEn, lang)
                : string.Empty;
        }
    }
}