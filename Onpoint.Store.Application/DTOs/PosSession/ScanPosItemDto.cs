namespace Onpoint.Store.Application.DTOs.Pos
{
    public class ScanPosItemDto
    {
        public int SessionId { get; set; }
        public string Code { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
    }
}