namespace Onpoint.Store.Application.DTOs.Contactmessage
{
    public class ContactMessageUpdateDto
    {

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsResolved { get; set; }
    }
}
