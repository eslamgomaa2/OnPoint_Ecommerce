namespace Onpoint.Store.Application.DTOs.PosSession
{
    public class ReceiptPreviewDto
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string CashierName { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public List<ReceiptItemDto> Items { get; set; } = new();
        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public decimal AmountReceived { get; set; }
        public decimal Change { get; set; }
        public List<SessionPaymentSummaryDto> Payments { get; set; } = new();
        public string? QrCodeData { get; set; }
    }
}
