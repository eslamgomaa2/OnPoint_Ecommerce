using Onpoint.Store.Domin.Common;

namespace Onpoint.Store.Domin.Entities
{
    public class AppSettings : BaseEntity
    {
        public string AppName { get; set; } = string.Empty;
        public string PrimaryColor { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
    }
}