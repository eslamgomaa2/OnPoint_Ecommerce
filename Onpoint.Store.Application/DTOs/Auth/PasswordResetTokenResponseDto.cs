namespace Onpoint.Store.Application.DTOs.Auth
{
    public class PasswordResetTokenResponseDto
    {
        public string ResetToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
