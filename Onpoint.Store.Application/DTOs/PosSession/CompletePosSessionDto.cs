namespace Onpoint.Store.Application.DTOs.PosSession
{
    public class CompletePosSessionDto
    {
        public List<SplitPaymentDto> Payments { get; set; } = new();
        public decimal AmountReceived { get; set; }




    }
}
