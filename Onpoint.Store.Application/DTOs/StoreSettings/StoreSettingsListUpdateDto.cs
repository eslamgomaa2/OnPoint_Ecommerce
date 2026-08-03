namespace Onpoint.Store.Application.DTOs.StoreSettings
{
    public class StoreSettingsListUpdateDto
    {
        public List<SettingKeyValueDto> Settings { get; set; } = new();
    }
}
