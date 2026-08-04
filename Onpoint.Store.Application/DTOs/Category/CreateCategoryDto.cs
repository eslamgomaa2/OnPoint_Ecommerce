namespace Onpoint.Store.Application.DTOs.Category
{
    public class CreateCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public string? NameEn { get; set; }          // ⬅️ جديد
        public string? Description { get; set; }
        public string? DescriptionEn { get; set; }   // ⬅️ جديد
        public bool IsActive { get; set; } = true;
    }
}