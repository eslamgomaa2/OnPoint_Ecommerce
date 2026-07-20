using BuildingBlocks.Results;
using Onpoint.Store.Application.DTOs.Stock;

namespace Onpoint.Store.Application.Services.StockServ
{
    public interface IStockService
    {
        Task<ServiceResult<StockDto>> InitializeStockAsync(InitializeStockDto dto, CancellationToken ct = default);
        Task<ServiceResult<StockDto>> AdjustStockAsync(AdjustStockDto dto, CancellationToken ct = default);
        Task<ServiceResult<bool>> TransferStockAsync(TransferStockDto dto, CancellationToken ct = default);
        Task<ServiceResult<List<StockDto>>> GetStockByProductAsync(int productId, CancellationToken ct = default);
        Task<ServiceResult<StockDto>> GetStockAsync(int productId, int? productVariantId, int branchId, CancellationToken ct = default);


        Task ReserveStockAsync(int productId, int? productVariantId, int branchId, int quantity, CancellationToken ct = default);
        Task ReleaseReservedStockAsync(int productId, int? productVariantId, int branchId, int quantity, CancellationToken ct = default);
        Task DecreaseStockAsync(int productId, int? productVariantId, int branchId, int quantity, CancellationToken ct = default);
    }
}