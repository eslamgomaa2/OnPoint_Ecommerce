using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace BuildingBlocks.Extensions
{
    public static class FluentValidationExtensions
    {
        public static IServiceCollection AddFluentValidationConfiguration(this IServiceCollection services)
        {
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    
                    var errors = context.ModelState
                        .Where(e => e.Value?.Errors.Count > 0)
                        .Select(e => $"{e.Key}: {e.Value?.Errors.First().ErrorMessage}")
                        .ToList();

                   
                    var response = new
                    {
                        Succeeded = false,
                        Message = "Data validation failed.",
                        Errors = errors
                    };

                    return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(response);
                };
            });

            return services;
        }
 
    }
}
