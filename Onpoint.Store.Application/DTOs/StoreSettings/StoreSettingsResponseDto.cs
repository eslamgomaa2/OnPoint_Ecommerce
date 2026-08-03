namespace Onpoint.Store.Application.DTOs.StoreSettings
{
    public class StoreSettingsResponseDto
    {
        public int Id { get; set; }
        public List<StoreSettingItemDto> Settings { get; set; } = new();
    }
}
