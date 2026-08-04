
using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Application.DTOs.ProductAttribute
{
    public class UpdateProductAttributeDto
    {
        public string Name { get; set; } = string.Empty;
        public string? NameEn { get; set; }
        public string Key { get; set; } = string.Empty;
        public AttributeValueType ValueType { get; set; } = AttributeValueType.Text;

    }
}