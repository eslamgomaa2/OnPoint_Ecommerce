using Microsoft.AspNetCore.Http;

namespace Onpoint.Store.Application.Services.Language
{
    public class LanguageService : ILanguageService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LanguageService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public bool IsEnglish
        {
            get
            {
                var lang = _httpContextAccessor.HttpContext?
                    .Request.Headers["Accept-Language"]
                    .FirstOrDefault();

                return lang?.StartsWith("en", StringComparison.OrdinalIgnoreCase) == true;
            }
        }
    }
}
