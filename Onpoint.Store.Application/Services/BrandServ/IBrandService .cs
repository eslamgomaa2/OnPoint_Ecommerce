using BuildingBlocks.Results;
using Onpoint.Store.Application.DTOs.Brand;
using Onpoint.Store.Application.DTOs.Product;

namespace Onpoint.Store.Application.Services.Brand
{
    public interface IBrandService
    {
        Task<ServiceResult<IEnumerable<ProductDto>>> GetProductsByBrandIdAsync(int brandId);
        Task<ServiceResult<IEnumerable<BrandDto>>> GetAllAsync();
        Task<ServiceResult<BrandDto>> GetByIdAsync(int id);
        Task<ServiceResult<(IReadOnlyList<BrandDto> Items, int TotalCount)>> GetFilteredPagedAsync(string? searchTerm, bool? isActive, int pageNumber, int pageSize);
        Task<ServiceResult<BrandDto>> CreateAsync(CreateBrandDto dto);
        Task<ServiceResult<BrandDto>> UpdateAsync(int id, UpdateBrandDto dto);
        Task<ServiceResult<bool>> ToggleActiveAsync(int id);
        Task<ServiceResult<bool>> DeleteAsync(int id);
    }
}