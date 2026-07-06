using BuildingBlocks.Common.Exceptions;
using BuildingBlocks.Localization;
using BuildingBlocks.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace BuildingBlocks.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ServiceResultHandler _serviceResultHandler;

        public GlobalExceptionMiddleware(RequestDelegate next, ServiceResultHandler serviceResultHandler)
        {
            _next = next;
            _serviceResultHandler = serviceResultHandler;
        }

        public async Task InvokeAsync(  HttpContext context, ILocalizationService localization,  ILogger<GlobalExceptionMiddleware> logger)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception occurred. Path: {Path}, Method: {Method}",
                    context.Request.Path, context.Request.Method);

               
                await HandleExceptionAsync(context, ex, localization);
            }
        }

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
            };

            if (statusCode == StatusCodes.Status401Unauthorized)
            {
                response.Message = message;
            }

           
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)response.HttpStatusCode;

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }

        private static (int StatusCode, string LocalizationKey) GetExceptionDetails(Exception exception)
        {
            return exception switch
            {
               
                UnauthorizedAccessException _ => (StatusCodes.Status401Unauthorized, "Errors.Unauthorized"),

                

               
                KeyNotFoundException _ => (StatusCodes.Status404NotFound, "Errors.NotFound"),

                
                InvalidImageException _ => (StatusCodes.Status400BadRequest, "Business.InvalidImage"),

              
                ArgumentException _ => (StatusCodes.Status400BadRequest, "Errors.ValidationFailed"),
                InvalidOperationException _ => (StatusCodes.Status400BadRequest, "Errors.ValidationFailed"),

               
                TimeoutException _ => (StatusCodes.Status504GatewayTimeout, "Errors.GenericError"),

                
                _ => (StatusCodes.Status500InternalServerError, "Errors.GenericError")
            };
        }
    }
}