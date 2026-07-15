using Onpoint.Store.Application.DTOs.Payment;

namespace Onpoint.Store.Application.Services.PaymentServ
{
    public interface IMyFatoorahClient
    {
        Task<List<PaymentMethodDto>> InitiatePaymentAsync(decimal amount, string currencyIso, CancellationToken ct = default);
        Task<ExecutePaymentResultDto> ExecutePaymentAsync(ExecutePaymentRequestModel request, CancellationToken ct = default);
        Task<MyFatoorahPaymentStatusResult> GetPaymentStatusAsync(string key, string keyType, CancellationToken ct = default);
    }
}