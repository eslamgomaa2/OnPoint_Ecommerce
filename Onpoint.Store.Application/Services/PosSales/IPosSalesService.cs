using BuildingBlocks.Results;
using Onpoint.Store.Application.DTOs.Order;
using Onpoint.Store.Application.DTOs.PosSales;
using Onpoint.Store.Application.DTOs.PosSession;
using Onpoint.Store.Application.DTOs.Refund;

namespace Onpoint.Store.Application.Services.PosSales
{
    public interface IPosSalesService
    {
        Task<ServiceResult<PosSalesListResponse>> GetPosSalesPagedAsync(PosSalesFilterRequest filter, int? forcedBranchId = null, CancellationToken ct = default);
        Task<ServiceResult<PosOrderDetailsDto>> GetOrderDetailsAsync(int orderId, CancellationToken ct = default);
        Task<ServiceResult<RefundDto>> FullRefundAsync(int orderId, FullRefundRequestDto dto, int? processedByUserId, CancellationToken ct = default);
        Task<ServiceResult<RefundDto>> PartialRefundAsync(int orderId, PartialRefundRequestDto dto, int? processedByUserId, CancellationToken ct = default);
        Task<ServiceResult<SalesReceiptPreviewDto>> GetReceiptAsync(int orderId, CancellationToken ct = default);
        Task<ServiceResult<byte[]>> GeneratePdfAsync(int orderId, CancellationToken ct = default);
        Task<ServiceResult<byte[]>> ExportToExcelAsync(ExportPosSalesRequestDto filter, int? forcedBranchId = null, CancellationToken ct = default);
        Task<ServiceResult<string>> GenerateAndUploadQrCodeAsync(int orderId, CancellationToken ct = default);
    }
}