using BuildingBlocks.Localization;
using Microsoft.Extensions.DependencyInjection;


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
