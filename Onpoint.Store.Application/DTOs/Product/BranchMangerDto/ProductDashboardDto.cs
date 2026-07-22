namespace Onpoint.Store.Application.DTOs.Product
{
    public class ProductDashboardDto
    {
        public int TotalProducts { get; set; }
        public int InStock { get; set; }
        public int LowStock { get; set; }
        public int OutOfStock { get; set; }
    }
}
