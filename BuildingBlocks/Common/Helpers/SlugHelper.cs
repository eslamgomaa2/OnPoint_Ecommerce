using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Onpoint.Store.Application.Helpers
{
    public static class SlugHelper
    {
        public static string GenerateSlug(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            string slug = input.Trim().ToLowerInvariant();

            slug = RemoveDiacritics(slug);

            slug = Regex.Replace(slug, @"[^a-z0-9\u0600-\u06FF\s-]", "");

            slug = Regex.Replace(slug, @"[\s-]+", "-");

            slug = slug.Trim('-');

            return slug;
        }

        public static string AppendSuffix(string baseSlug, int suffix)
        {
            return $"{baseSlug}-{suffix}";
        }

        private static string RemoveDiacritics(string text)
        {
            var normalized = text.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder();

            foreach (var c in normalized)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(c);
                if (category != UnicodeCategory.NonSpacingMark)
                    builder.Append(c);
            }

            return builder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}