namespace Onpoint.Store.Application.DTOs.Category
{
    public class UpdateCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? ParentCategoryId { get; set; }
        public int DisplayOrder { get; set; }
    }
}
