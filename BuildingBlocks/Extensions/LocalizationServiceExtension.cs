using BuildingBlocks.Localization;
using Microsoft.Extensions.DependencyInjection;
<<<<<<< HEAD

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

=======
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
    
>>>>>>> a4229cd5541012e96d4a2a23d93425d84f1837e3
}
