<<<<<<< HEAD
﻿using BuildingBlocks.Results;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
=======
﻿using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
>>>>>>> a4229cd5541012e96d4a2a23d93425d84f1837e3

namespace BuildingBlocks.Extensions
{
    public static class FluentValidationExtensions
    {
<<<<<<< HEAD
        public static IServiceCollection AddFluentValidationConfiguration(this IServiceCollection services, params Assembly[] assemblies)
        {
            if (assemblies == null || assemblies.Length == 0)
                throw new ArgumentNullException(nameof(assemblies), "Please provide the assemblies containing the validators.");

            services.AddValidatorsFromAssemblies(assemblies);

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {

                    var failures = context.ModelState
                        .Where(e => e.Value?.Errors.Count > 0)
                        .SelectMany(e => e.Value!.Errors.Select(err => new ValidationError
                        {
                            Property = e.Key,
                            Message = err.ErrorMessage,
                            AttemptedValue = context.HttpContext.Request.Form[e.Key]
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
=======
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
>>>>>>> a4229cd5541012e96d4a2a23d93425d84f1837e3
                };
            });

            return services;
        }
<<<<<<< HEAD
    }
}
=======
 
    }
}
>>>>>>> a4229cd5541012e96d4a2a23d93425d84f1837e3
