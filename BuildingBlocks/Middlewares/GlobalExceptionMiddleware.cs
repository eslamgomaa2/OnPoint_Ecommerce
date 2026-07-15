using BuildingBlocks.Common.Exceptions;
using BuildingBlocks.Localization;
using BuildingBlocks.Results;
<<<<<<< HEAD
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
=======
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
>>>>>>> a4229cd5541012e96d4a2a23d93425d84f1837e3
using System.Text.Json;

namespace BuildingBlocks.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
<<<<<<< HEAD

        public GlobalExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ILocalizationService localization, ILogger<GlobalExceptionMiddleware> logger)
=======
        private readonly ServiceResultHandler _serviceResultHandler;

        public GlobalExceptionMiddleware(RequestDelegate next, ServiceResultHandler serviceResultHandler)
        {
            _next = next;
            _serviceResultHandler = serviceResultHandler;
        }

        public async Task InvokeAsync(  HttpContext context, ILocalizationService localization,  ILogger<GlobalExceptionMiddleware> logger)
>>>>>>> a4229cd5541012e96d4a2a23d93425d84f1837e3
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception occurred. Path: {Path}, Method: {Method}",
                    context.Request.Path, context.Request.Method);

<<<<<<< HEAD
=======
               
>>>>>>> a4229cd5541012e96d4a2a23d93425d84f1837e3
                await HandleExceptionAsync(context, ex, localization);
            }
        }

<<<<<<< HEAD
        private async Task HandleExceptionAsync(HttpContext context, Exception exception, ILocalizationService localization)
        {
            context.Response.ContentType = "application/json";

            if (exception is ValidationException validationEx)
            {
                var errorsModel = new ValidationResultModel(validationEx.Errors);

                var response = new ServiceResult<ValidationResultModel>
                {
                    Succeeded = false,
                    Message = GetLocalizedMessage(localization, "Errors.ValidationFailed"),
                    Data = errorsModel,
                    HttpStatusCode = HttpStatusCode.UnprocessableEntity
                };

                context.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                await context.Response.WriteAsync(JsonSerializer.Serialize(response, GetJsonOptions()));
                return;
            }


            var (statusCode, localizationKey) = GetExceptionDetails(exception);
            string message = GetLocalizedMessage(localization, localizationKey);

            var standardResponse = new ServiceResult<object>
            {
                Succeeded = false,
                Message = message,
                HttpStatusCode = (HttpStatusCode)statusCode
=======
        private async Task HandleExceptionAsync( HttpContext context, Exception exception, ILocalizationService localization)
        {
            
            var (statusCode, localizationKey) = GetExceptionDetails(exception);

            
            string message;
            try
            {
                message = localization.Get(localizationKey);
            }
            catch
            {
                message = "An unexpected error occurred.";
            }

            ServiceResult<object> response = statusCode switch
            {
                StatusCodes.Status401Unauthorized => _serviceResultHandler.Unauthorized<object>(),
                StatusCodes.Status403Forbidden => _serviceResultHandler.Forbidden<object>(message),
                StatusCodes.Status404NotFound => _serviceResultHandler.NotFound<object>(message),
                StatusCodes.Status400BadRequest => _serviceResultHandler.BadRequest<object>(message),
                StatusCodes.Status422UnprocessableEntity => _serviceResultHandler.UnProcessableEntity<object>(message),
                _ => _serviceResultHandler.BadRequest<object>(message) 
>>>>>>> a4229cd5541012e96d4a2a23d93425d84f1837e3
            };

            if (statusCode == StatusCodes.Status401Unauthorized)
            {
<<<<<<< HEAD
                standardResponse.Message = message;
            }

            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsync(JsonSerializer.Serialize(standardResponse, GetJsonOptions()));
        }

        private static string GetLocalizedMessage(ILocalizationService localization, string key)
        {
            try
            {
                return localization.Get(key);
            }
            catch
            {
                return "An unexpected error occurred.";
            }
=======
                response.Message = message;
            }

           
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)response.HttpStatusCode;

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
>>>>>>> a4229cd5541012e96d4a2a23d93425d84f1837e3
        }

        private static (int StatusCode, string LocalizationKey) GetExceptionDetails(Exception exception)
        {
            return exception switch
            {
<<<<<<< HEAD
                UnauthorizedAccessException _ => (StatusCodes.Status401Unauthorized, "Errors.Unauthorized"),
                KeyNotFoundException _ => (StatusCodes.Status404NotFound, "Errors.NotFound"),
                InvalidImageException _ => (StatusCodes.Status400BadRequest, "Business.InvalidImage"),
                ArgumentException _ => (StatusCodes.Status400BadRequest, "Errors.ValidationFailed"),
                InvalidOperationException _ => (StatusCodes.Status400BadRequest, "Errors.ValidationFailed"),
                TimeoutException _ => (StatusCodes.Status504GatewayTimeout, "Errors.GenericError"),
                _ => (StatusCodes.Status500InternalServerError, "Errors.GenericError")
            };
        }

        private static JsonSerializerOptions GetJsonOptions() => new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
=======
               
                UnauthorizedAccessException _ => (StatusCodes.Status401Unauthorized, "Errors.Unauthorized"),

                

               
                KeyNotFoundException _ => (StatusCodes.Status404NotFound, "Errors.NotFound"),

                
                InvalidImageException _ => (StatusCodes.Status400BadRequest, "Business.InvalidImage"),

              
                ArgumentException _ => (StatusCodes.Status400BadRequest, "Errors.ValidationFailed"),
                InvalidOperationException _ => (StatusCodes.Status400BadRequest, "Errors.ValidationFailed"),

               
                TimeoutException _ => (StatusCodes.Status504GatewayTimeout, "Errors.GenericError"),

                
                _ => (StatusCodes.Status500InternalServerError, "Errors.GenericError")
            };
        }
>>>>>>> a4229cd5541012e96d4a2a23d93425d84f1837e3
    }
}