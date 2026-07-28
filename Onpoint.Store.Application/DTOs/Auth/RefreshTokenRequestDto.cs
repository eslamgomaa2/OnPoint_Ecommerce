using System.ComponentModel.DataAnnotations;

namespace Onpoint.Store.Application.DTOs.Auth
{
    public class RefreshTokenRequestDto
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
