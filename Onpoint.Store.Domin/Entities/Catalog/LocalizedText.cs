namespace Onpoint.Store.Domin.Entities.Catalog
{

    public record LocalizedText
    {
        public string En { get; set; } = string.Empty;
        public string Ar { get; set; } = string.Empty;

        public LocalizedText() { }

        public LocalizedText(string en, string ar)
        {
            En = en;
            Ar = ar;
        }

        // Helper للحصول على النص حسب اللغة
        public string Get(string culture = "en") =>
            culture.ToLower() switch
            {
                "ar" => Ar,
                _ => En
            };

        public string this[string culture] => Get(culture);
    }
}
