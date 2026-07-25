namespace Onpoint.Store.Application.DTOs.Auth
{
    public class ConfirmEmailDto
    {
        public string Email { get; set; }
        public string OTP { get; set; } = string.Empty;
    }
}
