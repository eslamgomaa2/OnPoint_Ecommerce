using BuildingBlocks.Results;
using Onpoint.Store.Application.DTOs.Refund;

namespace Onpoint.Store.Application.Interfaces
{
    public interface IRefundService
    {
        Task<ServiceResult<RefundDto>> CreateFullRefundAsync(int userId, int? branchId, int orderId, FullRefundRequestDto dto, CancellationToken ct = default);
        Task<ServiceResult<RefundDto>> CreatePartialRefundAsync(int userId, int? branchId, int orderId, PartialRefundRequestDto dto, CancellationToken ct = default);
        Task<ServiceResult<IReadOnlyList<RefundDto>>> GetOrderRefundsAsync(int orderId, CancellationToken ct = default);
    }
}