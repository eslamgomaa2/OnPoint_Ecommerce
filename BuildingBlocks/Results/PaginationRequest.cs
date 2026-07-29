
namespace BuildingBlocks.Results
{
    public class PaginationRequest
    {

        public int PageNumber { get; set; } = 1;

        private int _pageSize = 10;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > 100 || value <= 0) ? 10 : value;
        }

        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsRead { get; set; }
    }
}
