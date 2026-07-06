using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace BuildingBlocks.Extensions
{

    public static class AutoMapperExtensions
    {
        public static IServiceCollection AddAutoMapperServices(this IServiceCollection services)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            return services;
        }
    }
}
