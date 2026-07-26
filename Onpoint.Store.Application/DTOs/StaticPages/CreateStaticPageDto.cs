using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Application.DTOs.StaticPages
{
    public class CreateStaticPageDto
    {
        public PageType Type { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
