using AutoMapper;
using Onpoint.Store.Application.DTOs.Category;
using Onpoint.Store.Application.Mapping;
using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Application.Mappings.Resolvers
{
    public class CategoryNameResolver : IValueResolver<Category, CategoryDto, string>
    {
        public string Resolve(Category src, CategoryDto dest, string destMember, ResolutionContext context)
        {
            var lang = LocalizationHelper.GetLang(context);
            return LocalizationHelper.Pick(src.Name, src.NameEn, lang);
        }
    }

    public class CategoryDescriptionResolver : IValueResolver<Category, CategoryDto, string?>
    {
        public string? Resolve(Category src, CategoryDto dest, string destMember, ResolutionContext context)
        {
            var lang = LocalizationHelper.GetLang(context);
            return LocalizationHelper.PickNullable(src.Description, src.DescriptionEn, lang);
        }
    }
}