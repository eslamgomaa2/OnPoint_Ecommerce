using System.ComponentModel.DataAnnotations;

namespace Onpoint.Store.Application.DTOs.Brand
{
    public class UpdateBrandDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        public string? NameEn { get; set; }          // ⬅️ جديد

        public string? LogoUrl { get; set; }

        public bool IsActive { get; set; }

        public int DisplayOrder { get; set; }
    }
}
