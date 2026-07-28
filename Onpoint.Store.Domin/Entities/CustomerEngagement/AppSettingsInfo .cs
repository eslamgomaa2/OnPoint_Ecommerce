using Onpoint.Store.Domin.Common;
using System.ComponentModel.DataAnnotations;

namespace Onpoint.Store.Domin.Entities
{
    public class AppSettingsInfo : BaseEntity
    {
        [Required, MaxLength(100)]
        public string AppName { get; set; } = "Onpoint Store";

        [MaxLength(500)]
        public string? AppLogoUrl { get; set; }

        [MaxLength(50)]
        public string? PrimaryColor { get; set; }

        [MaxLength(50)]
        public string? SecondaryColor { get; set; }

        [MaxLength(50)]
        public string? AccentColor { get; set; }

        [MaxLength(20)]
        public string? DefaultLanguage { get; set; } = "ar";

        public bool MaintenanceMode { get; set; } = false;

        [MaxLength(500)]
        public string? MaintenanceMessage { get; set; }
    }
}