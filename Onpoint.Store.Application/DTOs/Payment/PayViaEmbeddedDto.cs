namespace Onpoint.Store.Application.DTOs.Payment
{
    public class PayViaEmbeddedDto
    {
        public int OrderId { get; set; }
        public string SessionId { get; set; } = string.Empty;
    }
}