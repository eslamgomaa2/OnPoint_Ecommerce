using Onpoint.Store.Application.Services.Language;

namespace Onpoint.Store.Application.Extensions
{
    public static class LocalizationExtensions
    {
        public static string Localize(
            this ILanguageService language,
            string ar,
            string? en)
        {
            return language.IsEnglish
                ? en ?? ar
                : ar;
        }
    }
}
