namespace Onpoint.Store.Application.DTOs.Category
{
    public class UpdateCategoryDto
    {

        public string Name { get; set; } = string.Empty;
        public string? NameEn { get; set; }
        public string? Description { get; set; }
        public string? DescriptionEn { get; set; }
        public bool IsActive { get; set; }
    }
}