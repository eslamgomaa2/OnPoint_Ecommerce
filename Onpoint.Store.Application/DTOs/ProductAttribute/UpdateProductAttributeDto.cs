
using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Application.DTOs.ProductAttribute
{
    public class UpdateProductAttributeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public AttributeValueType ValueType { get; set; } = AttributeValueType.Text;
        public List<int> CategoryIds { get; set; } = new();
    }
}