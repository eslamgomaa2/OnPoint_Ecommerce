using BuildingBlocks.Common.Exceptions;
using BuildingBlocks.Localization;
using BuildingBlocks.Results;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace BuildingBlocks.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public GlobalExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ILocalizationService localization, ILogger<GlobalExceptionMiddleware> logger)
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
            };

            if (statusCode == StatusCodes.Status401Unauthorized)
            {
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

        private static JsonSerializerOptions GetJsonOptions() => new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }
}