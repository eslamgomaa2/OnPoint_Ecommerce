namespace Onpoint.Store.Application.DTOs.Product
{
    public class ProductAttributeValueDto
    {
        public int ProductAttributeId { get; set; }
        public string AttributeName { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}