using BuildingBlocks.Common.Exceptions;
using BuildingBlocks.Localization;
using BuildingBlocks.Results;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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
        #region
        /* private async Task HandleExceptionAsync(HttpContext context, Exception exception, ILocalizationService localization)
         {
             context.Response.ContentType = "application/json";


             if (exception is ValidationException validationEx)
             {
                 var errorMessages = validationEx.Errors
                     .Select(e => e.ErrorMessage)
                     .Distinct()
                     .ToList();

                 string combinedMessage = errorMessages.Count > 0
                     ? string.Join(" | ", errorMessages)
                     : GetLocalizedMessage(localization, "Errors.ValidationFailed");

                 var response = new ServiceResult<object>
                 {
                     Succeeded = false,
                     Message = combinedMessage,
                     Data = null,
                     Errors = errorMessages,
                     HttpStatusCode = StatusCodes.Status400BadRequest
                 };

                 context.Response.StatusCode = StatusCodes.Status400BadRequest;
                 await context.Response.WriteAsync(JsonSerializer.Serialize(response, GetJsonOptions()));
                 return;
             }


             var (statusCode, localizationKey) = GetExceptionDetails(exception);
             string message = GetLocalizedMessage(localization, localizationKey);

             var standardResponse = new ServiceResult<object>
             {
                 Succeeded = false,
                 Message = message,
                 Data = null,
                 Errors = new List<string> { message },
                 HttpStatusCode = statusCode
             };

             context.Response.StatusCode = statusCode;
             await context.Response.WriteAsync(JsonSerializer.Serialize(standardResponse, GetJsonOptions()));
         }*/
        #endregion
        private async Task HandleExceptionAsync(HttpContext context, Exception exception, ILocalizationService localization)
        {
            context.Response.ContentType = "application/json";

            if (exception is ValidationException validationEx)
            {
                var errorMessages = validationEx.Errors
                    .Select(e => e.ErrorMessage)
                    .Distinct()
                    .ToList();

                string combinedMessage = errorMessages.Count > 0
                    ? string.Join(" | ", errorMessages)
                    : GetLocalizedMessage(localization, "Errors.ValidationFailed");

                var response = new ServiceResult<object>
                {
                    Succeeded = false,
                    Message = combinedMessage,
                    Data = null,
                    Errors = errorMessages,
                    HttpStatusCode = StatusCodes.Status400BadRequest
                };

                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync(JsonSerializer.Serialize(response, GetJsonOptions()));
                return;
            }

            var (statusCode, localizationKey) = GetExceptionDetails(exception);

            // ✅ الفيكس الأساسي هنا
            string message = BuildErrorMessage(exception, statusCode, localization, localizationKey);

            var standardResponse = new ServiceResult<object>
            {
                Succeeded = false,
                Message = message,
                Data = null,
                Errors = new List<string> { message },
                HttpStatusCode = statusCode
            };

            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsync(JsonSerializer.Serialize(standardResponse, GetJsonOptions()));
        }

        // ✅ جديد
        private static string BuildErrorMessage(Exception exception, int statusCode, ILocalizationService localization, string localizationKey)
        {
            // للأخطاء الـ 500 غير المتوقعة، نفضل نسيبها عامة حماية للمعلومات الحساسة
            // (تفاصيلها الكاملة موجودة في الـ logs أصلاً عن طريق logger.LogError فوق)
            if (statusCode == StatusCodes.Status500InternalServerError)
                return GetLocalizedMessage(localization, localizationKey);

            // لباقي الأخطاء (400, 401, 402, 404, 504) اللي هي أخطاء متوقعة ومقصودة،
            // نعرض الرسالة الحقيقية اللي انت كاتبها وقت الـ throw
            return !string.IsNullOrWhiteSpace(exception.Message)
                ? exception.Message
                : GetLocalizedMessage(localization, localizationKey);
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

                UserNotFoundException _ => (StatusCodes.Status401Unauthorized, "Errors.UserNotFound"),
                UnauthorizedAccessException _ => (StatusCodes.Status401Unauthorized, "Errors.Unauthorized"),
                AccountInactiveException _ => (StatusCodes.Status402PaymentRequired, "Errors.AccountInactive"),


                InvalidImageException _ => (StatusCodes.Status400BadRequest, "Business.InvalidImage"),
                ArgumentException _ => (StatusCodes.Status400BadRequest, "Errors.ValidationFailed"),
                InvalidOperationException _ => (StatusCodes.Status400BadRequest, "Errors.ValidationFailed"),


                KeyNotFoundException _ => (StatusCodes.Status404NotFound, "Errors.NotFound"),
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