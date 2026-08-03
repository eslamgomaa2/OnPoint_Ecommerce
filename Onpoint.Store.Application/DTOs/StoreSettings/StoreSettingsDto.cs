namespace Onpoint.Store.Application.DTOs.StoreSettings
{
    public class StoreSettingDto
    {
        public int Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}