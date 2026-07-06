using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BuildingBlocks.Localization
{
    
        public class LocalizationService : ILocalizationService
        {
            private readonly ILogger<LocalizationService> _logger;
            private readonly IHttpContextAccessor _httpContextAccessor;
            private readonly Dictionary<string, Dictionary<string, string>> _translations;

            
            private const string DefaultCulture = "ar";

            public LocalizationService(
                ILogger<LocalizationService> logger,
                IHttpContextAccessor httpContextAccessor)
            {
                _logger = logger;
                _httpContextAccessor = httpContextAccessor;
                _translations = new Dictionary<string, Dictionary<string, string>>();

                LoadTranslations();
            }

            public string Get(string key)
            {
                var culture = GetCurrentCulture();

                if (TryGetValue(culture, key, out var value))
                    return value;

                if (culture != DefaultCulture && TryGetValue(DefaultCulture, key, out var fallbackValue))
                    return fallbackValue;

                _logger.LogWarning("Localization key '{Key}' not found for culture '{Culture}'.", key, culture);
                return key;
            }

            public string Get(string key, params object?[] arguments)
            {
                var message = Get(key);

                try
                {
                    return string.Format(message, arguments);
                }
                catch (FormatException)
                {
                    return message; 
                }
            }

            
            private string GetCurrentCulture()
            {
                var context = _httpContextAccessor.HttpContext;
                if (context is null) return DefaultCulture;

                var acceptLanguage = context.Request.Headers["Accept-Language"].ToString();

                if (string.IsNullOrWhiteSpace(acceptLanguage)) return DefaultCulture;
                var culture = acceptLanguage.Split(',')[0].Trim().Split('-')[0];

              
                return _translations.ContainsKey(culture) ? culture : DefaultCulture;
            }

            
            private void LoadTranslations()
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceNames = assembly.GetManifestResourceNames()
                    .Where(name => name.EndsWith(".json", StringComparison.OrdinalIgnoreCase));

                foreach (var resourceName in resourceNames)
                {
                    var fileName = resourceName.Split('.').Reverse().Skip(1).First();

                    using var stream = assembly.GetManifestResourceStream(resourceName);
                    if (stream is null) continue;

                    using var reader = new StreamReader(stream);
                    var json = reader.ReadToEnd();

                    var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                    if (dict is not null)
                    {
                        _translations[fileName] = dict;
                    }
                }
            }

            private bool TryGetValue(string culture, string key, out string? value)
            {
                value = null;
                if (_translations.TryGetValue(culture, out var dict) && dict.TryGetValue(key, out var val))
                {
                    value = val;
                    return true;
                }
                return false;
            }
        }
    
}
