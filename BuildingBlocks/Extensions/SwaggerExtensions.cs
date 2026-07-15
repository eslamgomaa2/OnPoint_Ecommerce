using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BuildingBlocks.Extensions
{
<<<<<<< HEAD
    public static class SwaggerExtensions
    {
        public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Onpoint Store API",
                    Version = "v1",
                    Description = "E-Commerce Backend API"
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter: {your_token}"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
=======
        public static class SwaggerExtensions
        {
            public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
            {
                services.AddSwaggerGen(options =>
                {
                    options.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Title = "Onpoint Store API",
                        Version = "v1",
                        Description = "E-Commerce Backend API"
                    });

                    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Description = "Enter: Bearer {your_token}"
                    });

                    options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
>>>>>>> a4229cd5541012e96d4a2a23d93425d84f1837e3
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
<<<<<<< HEAD
            });
            });

            return services;
        }
    }

=======
                });
                });

                return services;
            }
        }
    
>>>>>>> a4229cd5541012e96d4a2a23d93425d84f1837e3
}
