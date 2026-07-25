namespace Onpoint.Store.Application.DTOs.PosSales
{
    public class TimelineEventDto
    {
        public string Event { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
