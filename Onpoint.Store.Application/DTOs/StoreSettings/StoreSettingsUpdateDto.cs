namespace Onpoint.Store.Application.DTOs.StoreSettings
{
    public class StoreSettingsUpdateDto
    {
        public Dictionary<string, string> Settings { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }
}
