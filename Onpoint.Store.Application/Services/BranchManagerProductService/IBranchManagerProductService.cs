using BuildingBlocks.Results;
using Onpoint.Store.Application.DTOs.Product;
using Onpoint.Store.Application.DTOs.Product.BranchManger;

namespace Onpoint.Store.Application.Services.BranchManagerProductService
{
    public interface IBranchManagerProductService
    {
        Task<ServiceResult<ProductDashboardDto>> GetDashboardCountsAsync(int branchId, CancellationToken ct = default);
        Task<ServiceResult<PagedResult<ProductDto>>> GetFilteredPagedAsync(int branchId, PaginationRequest request, int? categoryId, string? searchTerm, CancellationToken ct = default);
        Task<ServiceResult<ProductDetailDto>> GetByIdAsync(int branchId, int id, CancellationToken ct = default);
        Task<ServiceResult<ProductDto>> CreateAsync(int branchId, CreateProductByBranchManagerDto dto, CancellationToken ct = default);
        Task<ServiceResult<ProductDto>> UpdateAsync(int branchId, int id, UpdateProductDto dto, CancellationToken ct = default);
        Task<ServiceResult<string>> DeleteAsync(int branchId, int id, CancellationToken ct = default);
    }
}