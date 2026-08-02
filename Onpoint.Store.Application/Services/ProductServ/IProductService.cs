using BuildingBlocks.Results;
using Onpoint.Store.Application.DTOs;
using Onpoint.Store.Application.DTOs.Product;
using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Application.Services.ProductServ
{
    public interface IProductService
    {
        Task<ServiceResult<ProductDashboardDto>> GetDashboardCountsAsync(int? branchId = null, CancellationToken ct = default);
        Task<ServiceResult<PagedResult<ProductListItemDto>>> GetFilteredAsync(
       ProductFilterRequestDto filter,
       CancellationToken ct = default);
        Task<ServiceResult<PagedResult<ProductDto>>> GetFilteredPagedAsync(
        PaginationRequest request, int? categoryId = null, string? searchTerm = null,
     int? branchId = null, LanguageCode? languageCode = null, int? currentUserId = null,
     CancellationToken ct = default);
        Task<ServiceResult<ProductDetailDto>> GetByIdAsync(int id, LanguageCode? languageCode = null, int? currentUserId = null, CancellationToken ct = default);

        Task<ServiceResult<ProductDto>> CreateAsync(CreateProductDto dto, CancellationToken ct = default);

        Task<ServiceResult<ProductDto>> UpdateAsync(int id, UpdateProductDto dto, CancellationToken ct = default);

        Task<ServiceResult<string>> DeleteAsync(int id, CancellationToken ct = default);

        Task<ServiceResult<ProductDto>> GetBySkuAsync(string sku, CancellationToken ct = default);
    }
}