using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Application.DTOs.Product
{
    public class ProductFilterRequestDto
    {

        public int? CategoryId { get; set; }
        public int? MinRating { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool? InStockOnly { get; set; }
        public string? Search { get; set; }
        public SortBy SortBy { get; set; } = SortBy.newest;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 12;
    }
}
