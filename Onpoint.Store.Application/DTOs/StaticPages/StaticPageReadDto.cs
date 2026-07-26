namespace Onpoint.Store.Application.DTOs.StaticPage
{
    public class StaticPageReadDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}