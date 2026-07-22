using System.ComponentModel.DataAnnotations;

namespace Onpoint.Store.Application.DTOs.Brand
{
    public class CreateBrandDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? LogoUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public int DisplayOrder { get; set; } = 0;
    }
}
