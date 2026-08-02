// Application/DTOs/ProductAttribute/ProductAttributeFilter.cs
using BuildingBlocks.Results;
using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Application.DTOs.ProductAttribute
{
    public class ProductAttributeFilter : PaginationRequest
    {
        public bool? IsActive { get; set; }
        public AttributeValueType? ValueType { get; set; }
    }
}