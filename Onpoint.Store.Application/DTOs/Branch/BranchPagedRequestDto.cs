using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Application.DTOs.Branch
{
    public class BranchPagedRequestDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public StatusFilter? StatusFilter { get; set; }
        public bool? IsDefaultFilter { get; set; }
        public SortColumn? SortBy { get; set; }
        public SortDirection? SortDirection { get; set; }
    }





}
