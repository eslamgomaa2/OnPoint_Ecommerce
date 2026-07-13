namespace Onpoint.Store.Application.DTOs.Product
{
    public class DiscountDto
    {
        public int Id { get; set; }
        public decimal DiscountPercentage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
