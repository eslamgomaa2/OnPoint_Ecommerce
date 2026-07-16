using BuildingBlocks.Results;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
namespace BuildingBlocks.Extensions
{
    public static class FluentValidationExtensions
    {
        public static IServiceCollection AddFluentValidationConfiguration(this IServiceCollection services, params Assembly[] assemblies)
        {
            if (assemblies == null || assemblies.Length == 0)
                throw new ArgumentNullException(nameof(assemblies), "Please provide the assemblies containing the validators.");

            services.AddValidatorsFromAssemblies(assemblies);

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var hasForm = context.HttpContext.Request.HasFormContentType;

                    var failures = context.ModelState
                        .Where(e => e.Value?.Errors.Count > 0)
                        .SelectMany(e => e.Value!.Errors.Select(err => new ValidationError
                        {
                            Property = e.Key,
                            Message = err.ErrorMessage,
                            AttemptedValue = hasForm ? context.HttpContext.Request.Form[e.Key].ToString() : null
                        }))
                        .ToList();

                    var validationModel = new ValidationResultModel
                    {
                        Errors = failures
                    };

                    var response = new ServiceResult<ValidationResultModel>
                    {
                        Succeeded = false,
                        Message = "Data validation failed.",
                        Data = validationModel,
                        HttpStatusCode = System.Net.HttpStatusCode.UnprocessableEntity
                    };

                    return new UnprocessableEntityObjectResult(response);
                };
            });

            return services;
        }
    }
}