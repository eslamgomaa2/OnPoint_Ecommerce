using BuildingBlocks.Results;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBuildingBlocksServices(this IServiceCollection services)
        {

            services.AddHttpContextAccessor();
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            services.AddHttpContextAccessor();

            services.AddScoped<ServiceResultHandler>();

            return services;
        }
    }
}