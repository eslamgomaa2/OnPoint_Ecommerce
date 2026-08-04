using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Common
{
    public class CurrentLanguage : ICurrentLanguage
    {
        public string Lang { get; }

        public CurrentLanguage(IHttpContextAccessor accessor)
        {
            var header = accessor.HttpContext?.Request.Headers["Accept-Language"].ToString();
            Lang = string.IsNullOrWhiteSpace(header) ? "ar"
                 : header.StartsWith("en", StringComparison.OrdinalIgnoreCase) ? "en" : "ar";
        }
    }
}
