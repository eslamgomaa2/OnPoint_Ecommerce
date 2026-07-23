using BuildingBlocks.Results;
using Onpoint.Store.Application.DTOs.Category;

namespace Onpoint.Store.Application.Services.CategoryServ
{
    public interface ICategoryService
    {
        Task<ServiceResult<IReadOnlyList<CategoryDto>>> GetAllAsync(CancellationToken ct = default);
        Task<ServiceResult<PagedResult<CategoryDto>>> GetPagedAsync(PaginationRequest request, CancellationToken ct = default);
        Task<ServiceResult<CategoryDto>> GetByIdAsync(int id, CancellationToken ct = default);
        Task<ServiceResult<CategoryDto>> CreateAsync(CreateCategoryDto dto, CancellationToken ct = default);
        Task<ServiceResult<CategoryDto>> UpdateAsync(int id, UpdateCategoryDto dto, CancellationToken ct = default);
        Task<ServiceResult<string>> DeleteAsync(int id, CancellationToken ct = default);
    }
}