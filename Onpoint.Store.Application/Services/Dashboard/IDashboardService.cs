using BuildingBlocks.Results;
using Onpoint.Store.Application.DTOs.Common;
using Onpoint.Store.Application.DTOs.DashBoard;

namespace Onpoint.Store.Application.Services
{
    public interface IDashboardService
    {
        Task<ServiceResult<SalesOverviewDto>> GetSalesOverviewAsync(int? branchId, CancellationToken ct = default);
        Task<ServiceResult<OrderStatusDistributionDto>> GetOrderStatusDistributionAsync(int? branchId, CancellationToken ct = default);
        Task<ServiceResult<DashboardHighlightsDto>> GetHighlightsAsync(int? branchId, CancellationToken ct = default);


        Task<ServiceResult<List<TopCustomerDto>>> GetTopCustomersAsync(int count, int? branchId, CancellationToken ct = default);
        Task<ServiceResult<List<TopBranchDto>>> GetTopBranchesAsync(int count, CancellationToken ct = default);
        Task<ServiceResult<RevenueBySourceDto>> GetRevenueBySourceAsync(int? branchId, CancellationToken ct = default);
        Task<ServiceResult<MetricDto>> GetPosSalesAsync(int? branchId, CancellationToken ct = default);
        Task<ServiceResult<MetricDto>> GetCompletedOnlineOrdersAsync(int? branchId, CancellationToken ct = default);
        Task<ServiceResult<MetricDto>> GetTotalDiscountRevenueAsync(int? branchId, CancellationToken ct = default);

        Task<ServiceResult<PagedResultDto<LatestOrderDto>>> GetLatestOrdersAsync(int pageNumber, int pageSize, int? branchId, CancellationToken ct = default);
        Task<ServiceResult<OrdersBySourceDto>> GetOrdersBySourceAsync(int? branchId, CancellationToken ct = default);
        Task<ServiceResult<List<TopSellingProductDto>>> GetTopSellingProductsAsync(int count, int? branchId, CancellationToken ct = default);
    }

}