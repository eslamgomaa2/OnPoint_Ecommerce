using AutoMapper;
using Onpoint.Store.Application.DTOs.StaticPage;
using Onpoint.Store.Application.DTOs.StaticPages;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Application.Mappings
{
    public class StaticPageMappingProfile : Profile
    {
        public StaticPageMappingProfile()
        {
            CreateMap<StaticPage, StaticPageReadDto>()
                .ForMember(d => d.Type, opt => opt.MapFrom(s => MapTypeToString(s.Type)));

            CreateMap<CreateStaticPageDto, StaticPage>();

            CreateMap<UpdateStaticPageDto, StaticPage>()
                .ForMember(d => d.Type, opt => opt.Ignore());
        }

        private static string MapTypeToString(PageType type) => type switch
        {
            PageType.Privacy => "privacy",
            PageType.Terms => "terms",
            PageType.AboutUs => "about",
            PageType.ContactUs => "contact",
            _ => type.ToString().ToLower()
        };
    }
}