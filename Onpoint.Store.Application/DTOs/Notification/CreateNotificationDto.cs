namespace Onpoint.Store.Application.DTOs.Notification
{
    public class CreateNotificationDto
    {
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public int? UserId { get; set; }
    }
}
