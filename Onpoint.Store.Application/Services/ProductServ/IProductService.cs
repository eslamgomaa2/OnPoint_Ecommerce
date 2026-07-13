using BuildingBlocks.Results;
using Onpoint.Store.Application.DTOs.Product;

namespace Onpoint.Store.Application.Services.ProductServ
{
    public interface IProductService
    {

        Task<ServiceResult<PagedResult<ProductDto>>> GetFilteredPagedAsync(PaginationRequest request, int? categoryId = null, string? searchTerm = null, CancellationToken ct = default);


        Task<ServiceResult<ProductDetailDto>> GetByIdAsync(int id, CancellationToken ct = default);

        Task<ServiceResult<ProductDto>> CreateAsync(CreateProductDto dto, CancellationToken ct = default);
        Task<ServiceResult<ProductDto>> UpdateAsync(UpdateProductDto dto, CancellationToken ct = default);
        Task<ServiceResult<string>> DeleteAsync(int id, CancellationToken ct = default);
    }
}
