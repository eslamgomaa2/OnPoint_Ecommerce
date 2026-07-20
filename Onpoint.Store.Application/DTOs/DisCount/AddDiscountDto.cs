namespace Onpoint.Store.Application.DTOs.Discount
{
    public class AddDiscountDto
    {
        public int ProductId { get; set; }
        public decimal DiscountPercentage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}