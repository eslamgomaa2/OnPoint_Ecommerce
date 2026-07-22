namespace Onpoint.Store.Application.DTOs.Order
{
    public class GetOrdersQueryDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public OrderSortBy SortBy { get; set; } = OrderSortBy.Date;
        public bool Descending { get; set; } = true;
    }
}