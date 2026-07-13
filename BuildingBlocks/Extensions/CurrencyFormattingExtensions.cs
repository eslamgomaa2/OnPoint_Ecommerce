using BuildingBlocks.Settings;
using System.Globalization;

namespace BuildingBlocks.Extensions
{
    public static class CurrencyFormattingExtensions
    {
        public static string ToCurrencyString(this decimal amount, CurrencySettings currency, string culture = "ar-KW")
        {
            var cultureInfo = new CultureInfo(culture);
            return amount.ToString($"N{currency.DecimalPlaces}", cultureInfo) + " " + currency.Symbol;

        }
    }
}