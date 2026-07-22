using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Application.DTOs.Product
{
    public class UpdateProductTranslationDto
    {
        public int Id { get; set; }
        public LanguageCode LanguageCode { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
    }
}