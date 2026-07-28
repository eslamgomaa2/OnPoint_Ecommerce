namespace Onpoint.Store.Application.DTOs.AppSettings
{
    public class AppSettingsDto
    {
        public int Id { get; set; }
        public string AppName { get; set; } = string.Empty;
        public string? AppLogoUrl { get; set; }
        public string PrimaryColor { get; set; } = string.Empty;
        public string? SecondaryColor { get; set; }
        public string? AccentColor { get; set; }
        public string DefaultLanguage { get; set; } = "ar";
        public bool MaintenanceMode { get; set; }
        public string? MaintenanceMessage { get; set; }
    }



}