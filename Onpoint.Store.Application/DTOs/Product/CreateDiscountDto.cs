namespace Onpoint.Store.Application.DTOs.Product
{
    public class CreateDiscountDto
    {
        public decimal DiscountPercentage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
