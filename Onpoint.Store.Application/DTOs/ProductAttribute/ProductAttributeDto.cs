using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Application.DTOs.ProductAttribute
{
    public class ProductAttributeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public AttributeValueType ValueType { get; set; }
        public List<CategoryBriefDto> Categories { get; set; } = new();
    }
}
