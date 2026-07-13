using System.ComponentModel.DataAnnotations;

namespace Onpoint.Store.Application.DTOs.Product
{
    public class CreateProductImageDto
    {
        [Url]
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
    }
}
