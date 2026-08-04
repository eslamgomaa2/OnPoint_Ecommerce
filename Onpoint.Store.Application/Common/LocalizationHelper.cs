using AutoMapper;

namespace Onpoint.Store.Application.Mapping
{
    public static class LocalizationHelper
    {
        public static string Pick(string? ar, string? en, string? lang) =>
            lang == "en" && !string.IsNullOrWhiteSpace(en) ? en! : (ar ?? string.Empty);

        public static string? PickNullable(string? ar, string? en, string? lang) =>
            lang == "en" && !string.IsNullOrWhiteSpace(en) ? en : ar;

        public static string GetLang(ResolutionContext ctx) =>
            ctx.Items.TryGetValue("lang", out var l) ? l?.ToString() ?? "ar" : "ar";
    }
}