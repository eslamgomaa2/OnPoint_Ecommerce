using Microsoft.Extensions.DependencyInjection;
using Onpoint.Store.Domin.Repositories;
using Onpoint.Store.Infrastructure.Repositories;

namespace Onpoint.Store.Infrastructure.Extensions
{
    
        public static class InfrastructureServiceExtension
        {
            public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
            {
                
                services.AddScoped<IUnitOfWork, UnitOfWork>();

                return services;
            }
        }
    
}
