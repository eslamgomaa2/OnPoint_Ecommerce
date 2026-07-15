public class ExecutePaymentRequestModel
{
    public int? PaymentMethodId { get; set; } // Hosted flow
    public string? SessionId { get; set; }     // Embedded flow
    public decimal InvoiceValue { get; set; }
    public string DisplayCurrencyIso { get; set; } = "KWD";
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerMobile { get; set; } = string.Empty;
    public string MobileCountryCode { get; set; } = "+965";
    public string CallBackUrl { get; set; } = string.Empty;
    public string ErrorUrl { get; set; } = string.Empty;
    public string CustomerReference { get; set; } = string.Empty; // هنحط OrderId هنا
    public string Language { get; set; } = "en";
}
