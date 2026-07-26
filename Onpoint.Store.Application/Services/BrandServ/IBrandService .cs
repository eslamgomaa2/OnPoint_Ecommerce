using BuildingBlocks.Results;
using Onpoint.Store.Application.DTOs.Brand;
using Onpoint.Store.Application.DTOs.Common;
using Onpoint.Store.Application.DTOs.Product;

namespace Onpoint.Store.Application.Services.Brand
{
    public interface IBrandService
    {
        Task<ServiceResult<IEnumerable<ProductDto>>> GetProductsByBrandIdAsync(int brandId, CancellationToken ct = default);
        Task<ServiceResult<IEnumerable<BrandDto>>> GetAllAsync(CancellationToken ct = default);
        Task<ServiceResult<BrandDto>> GetByIdAsync(int id, CancellationToken ct = default);
        Task<ServiceResult<PagedResultDto<BrandDto>>> GetFilteredPagedAsync(string? searchTerm, bool? isActive, int pageNumber, int pageSize, CancellationToken ct = default);
        Task<ServiceResult<BrandDto>> CreateAsync(CreateBrandDto dto, CancellationToken ct = default);
        Task<ServiceResult<BrandDto>> UpdateAsync(int id, UpdateBrandDto dto, CancellationToken ct = default);
        Task<ServiceResult<bool>> ToggleActiveAsync(int id, CancellationToken ct = default);
        Task<ServiceResult<bool>> DeleteAsync(int id, CancellationToken ct = default);
    }
}