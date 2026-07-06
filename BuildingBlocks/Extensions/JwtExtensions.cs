
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace BuildingBlocks.Extensions
{
  
        public static class JwtExtensions
        {
            public static IServiceCollection AddJwtAuthentication( this IServiceCollection services, IConfiguration configuration)
            {
                var jwtSettings = configuration.GetSection("JwtSettings");
                var secretKey = jwtSettings["SecretKey"]
                    ?? throw new InvalidOperationException("JWT SecretKey not configured");

                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.SaveToken = true;
                    options.RequireHttpsMetadata = false;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings["Issuer"] ?? "OnpointStore",
                        ValidAudience = jwtSettings["Audience"] ?? "OnpointStoreClients",
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                        ClockSkew = TimeSpan.Zero
                    };
                });

               
                return services;
            }
        }
    
}
