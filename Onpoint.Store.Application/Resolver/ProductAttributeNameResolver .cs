using AutoMapper;
using Onpoint.Store.Application.DTOs.ProductAttribute;
using Onpoint.Store.Application.Mapping;
using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Application.Mappings.Resolvers
{
    public class ProductAttributeNameResolver : IValueResolver<ProductAttribute, ProductAttributeDto, string>
    {
        public string Resolve(ProductAttribute src, ProductAttributeDto dest, string destMember, ResolutionContext context)
        {
            var lang = LocalizationHelper.GetLang(context);
            return LocalizationHelper.Pick(src.Name, src.NameEn, lang);
        }
    }

    public class CategoryBriefNameResolver : IValueResolver<Category, CategoryBriefDto, string>
    {
        public string Resolve(Category src, CategoryBriefDto dest, string destMember, ResolutionContext context)
        {
            var lang = LocalizationHelper.GetLang(context);
            return LocalizationHelper.Pick(src.Name, src.NameEn, lang);
        }
    }
}