namespace Onpoint.Store.Application.DTOs.Product
{
    public class ProductTranslationDto
    {
        public int Id { get; set; }
        public string LanguageCode { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
    }
}