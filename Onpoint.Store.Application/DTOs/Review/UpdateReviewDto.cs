namespace Onpoint.Store.Application.DTOs.Review
{
    public class UpdateReviewDto
    {
        public int ReviewId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
}