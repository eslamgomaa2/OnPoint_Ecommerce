using BuildingBlocks.Localization;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlocks.Extensions
{
    
        public static class LocalizationServiceExtension
        {
            public static IServiceCollection AddLocalizationServices(this IServiceCollection services)
            {
                services.AddHttpContextAccessor();

                services.AddSingleton<ILocalizationService, LocalizationService>();

                return services;
            }
        }
    
}
