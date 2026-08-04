using BuildingBlocks.Results;
using Onpoint.Store.Application.DTOs.Payment;

namespace Onpoint.Store.Application.Services.PaymentServices
{
    public interface IPaymentService
    {
        Task<ServiceResult<List<PaymentMethodDto>>> GetAvailablePaymentMethodsAsync(int sessionId, CancellationToken ct = default);
        Task<ServiceResult<ExecutePaymentResultDto>> PayViaHostedAsync(int Customerid, PayViaHostedDto dto, CancellationToken ct = default);
        //Task<ServiceResult<ExecutePaymentResultDto>> PayViaEmbeddedAsync(int userId, PayViaEmbeddedDto dto, CancellationToken ct = default);
        Task HandleWebhookNotificationAsync(string invoiceIdOrPaymentId, CancellationToken ct = default);
        Task<ServiceResult<PaymentStatusDto>> CheckPaymentStatusAsync(string invoiceId, CancellationToken ct = default);
    }
}
