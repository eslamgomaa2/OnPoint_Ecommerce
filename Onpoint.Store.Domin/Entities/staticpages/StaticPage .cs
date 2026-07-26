using Onpoint.Store.Domin.Common;
using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Domin.Entities
{
    public class StaticPage : BaseEntity
    {
        public PageType Type { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}