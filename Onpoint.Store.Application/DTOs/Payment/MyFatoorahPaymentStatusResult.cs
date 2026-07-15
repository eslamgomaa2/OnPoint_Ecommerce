using Onpoint.Store.Application.DTOs.Payment;

public class MyFatoorahPaymentStatusResult
{
    public string InvoiceId { get; set; } = string.Empty;
    public string InvoiceStatus { get; set; } = string.Empty;
    public decimal InvoiceValue { get; set; }
    public string? CustomerReference { get; set; }
    public MyFatoorahTransactionDetail? InvoiceTransactions { get; set; }
}