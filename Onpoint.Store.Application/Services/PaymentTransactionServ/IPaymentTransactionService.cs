using BuildingBlocks.Results;
using Onpoint.Store.Application.DTOs.PaymentTransaction;

namespace Onpoint.Store.Application.Services.PaymentTransactionServ
{
    public interface IPaymentTransactionService
    {
        Task<ServiceResult<PaymentTransactionDto>> UpdateStatusAsync(int id, UpdatePaymentStatusDto dto);
        Task<ServiceResult<PaymentTransactionDto>> GetByOrderIdAsync(int orderId);
    }
}