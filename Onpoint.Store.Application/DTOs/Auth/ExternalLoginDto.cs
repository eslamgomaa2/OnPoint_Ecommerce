namespace Onpoint.Store.Application.DTOs.Auth
{
    public class ExternalLoginDto
    {
        public string Provider { get; set; } = string.Empty;
        public string IdToken { get; set; } = string.Empty;
    }
}
