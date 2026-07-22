
using Onpoint.Store.Domin.Common;
using Onpoint.Store.Domin.Enums;
using System.ComponentModel.DataAnnotations;

namespace Onpoint.Store.Domin.Entities
{
    public class ProductTranslation : BaseEntity
    {
        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        [Required, MaxLength(10)]
        public LanguageCode LanguageCode { get; set; }

        [MaxLength(200)]
        public string? Name { get; set; }

        public string? Description { get; set; }

        [MaxLength(200)]
        public string? MetaTitle { get; set; }

        [MaxLength(500)]
        public string? MetaDescription { get; set; }
    }
}