using AutoMapper;
using Onpoint.Store.Application.DTOs.Brand;
using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Application.Mapping.Resolvers
{
    public class BrandNameResolver : IValueResolver<Brand, BrandDto, string>
    {
        public string Resolve(Brand src, BrandDto dest, string destMember, ResolutionContext context)
        {
            var lang = LocalizationHelper.GetLang(context);
            return LocalizationHelper.Pick(src.Name, src.NameEn, lang);
        }
    }
}