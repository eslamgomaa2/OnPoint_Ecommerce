namespace Onpoint.Store.Application.DTOs.PosSession
{
    public class CompletePosSessionDto
    {
        public List<SplitPaymentDto> Payments { get; set; } = new();
        public decimal AmountReceived { get; set; }
        public int? CustomerId { get; set; }
        public string? CustomerPhone { get; set; }
        public string? Note { get; set; }

    }
}
