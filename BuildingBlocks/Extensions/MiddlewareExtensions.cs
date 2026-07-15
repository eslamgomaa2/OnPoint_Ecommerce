using BuildingBlocks.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace BuildingBlocks.Extensions
{
<<<<<<< HEAD

    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
        {
            return app.UseMiddleware<GlobalExceptionMiddleware>();
        }

        public static IApplicationBuilder UseRequestLocalizationConfiguration(this IApplicationBuilder app)
        {
            var supportedCultures = new[] { "en", "ar" };

            var options = new RequestLocalizationOptions()
                .SetDefaultCulture("en")
                .AddSupportedCultures(supportedCultures)
                .AddSupportedUICultures(supportedCultures);

            return app.UseRequestLocalization(options);
        }
    }

=======
   
        public static class MiddlewareExtensions
        {
            public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
            {
                return app.UseMiddleware<GlobalExceptionMiddleware>();
            }

            public static IApplicationBuilder UseRequestLocalizationConfiguration(this IApplicationBuilder app)
            {
                var supportedCultures = new[] { "en", "ar" };

                var options = new RequestLocalizationOptions()
                    .SetDefaultCulture("en")
                    .AddSupportedCultures(supportedCultures)
                    .AddSupportedUICultures(supportedCultures);

                return app.UseRequestLocalization(options);
            }
        }
    
>>>>>>> a4229cd5541012e96d4a2a23d93425d84f1837e3
}
