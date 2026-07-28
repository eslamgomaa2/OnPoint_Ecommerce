using AutoMapper;
using Onpoint.Store.Application.DTOs.AppSettings;
using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Application.Mapping
{
    public class AppSettingsProfile : Profile
    {
        public AppSettingsProfile()
        {
            CreateMap<AppSettingsInfo, AppSettingsDto>();
        }
    }
}