using BuildingBlocks.Results;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBuildingBlocksServices(this IServiceCollection services)
        {
<<<<<<< HEAD

            services.AddHttpContextAccessor();
            services.AddHttpClient();
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });
=======
            
            services.AddHttpContextAccessor();
>>>>>>> a4229cd5541012e96d4a2a23d93425d84f1837e3
            services.AddScoped<ServiceResultHandler>();

            return services;
        }
    }
}