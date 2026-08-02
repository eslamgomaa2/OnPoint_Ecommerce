namespace Onpoint.Store.Application.DTOs.Discount
{
    public class UpdateDiscountDto
    {

        public decimal DiscountPercentage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}