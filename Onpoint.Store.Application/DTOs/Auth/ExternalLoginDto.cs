namespace Onpoint.Store.Application.DTOs.Auth
{
    public class ExternalLoginDto
    {
        public string Provider { get; set; } = string.Empty; // "Google", "Facebook", "Apple"
        public string IdToken { get; set; } = string.Empty;  // الرمز الذي يرسله الـ Frontend من جوجل/فيس بوك/آبل
    }
}
