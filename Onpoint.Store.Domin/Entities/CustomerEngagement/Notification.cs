namespace Onpoint.Store.Domin.Entities.CustomerEngagement
{
    public class Notification
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public int? UserId { get; set; }
    }
}
