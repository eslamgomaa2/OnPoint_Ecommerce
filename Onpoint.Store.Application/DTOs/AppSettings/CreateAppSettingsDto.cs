using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Onpoint.Store.Application.DTOs.AppSettings
{

    public class CreateAppSettingsDto
    {
        [Required]
        public string AppName { get; set; } = string.Empty;

        public IFormFile? LogoFile { get; set; }

        public string PrimaryColor { get; set; } = "#3B82F6";

        public string? SecondaryColor { get; set; }

        public string? AccentColor { get; set; }

        public string DefaultLanguage { get; set; } = "ar";

        public bool MaintenanceMode { get; set; } = false;

        public string? MaintenanceMessage { get; set; }
    }






}