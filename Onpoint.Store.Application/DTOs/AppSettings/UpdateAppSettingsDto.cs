using Microsoft.AspNetCore.Http;

namespace Onpoint.Store.Application.DTOs.AppSettings
{
    public class UpdateAppSettingsDto
    {
        public string? AppName { get; set; }

        public IFormFile? LogoFile { get; set; }

        public string? PrimaryColor { get; set; }

        public string? SecondaryColor { get; set; }

        public string? AccentColor { get; set; }

        public string? DefaultLanguage { get; set; }

        public bool? MaintenanceMode { get; set; }

        public string? MaintenanceMessage { get; set; }
    }

}
