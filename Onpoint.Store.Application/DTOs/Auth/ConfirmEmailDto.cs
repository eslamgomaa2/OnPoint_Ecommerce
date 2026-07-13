namespace Onpoint.Store.Application.DTOs.Auth
{
    public class ConfirmEmailDto
    {
        public int UserId { get; set; }
        public string Token { get; set; } = string.Empty;
    }
}
