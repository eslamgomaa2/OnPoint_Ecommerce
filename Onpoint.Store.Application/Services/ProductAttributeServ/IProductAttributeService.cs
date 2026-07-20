using BuildingBlocks.Results;
using Onpoint.Store.Application.DTOs.ProductAttribute;

namespace Onpoint.Store.Application.Services.ProductAttributeServ
{
    public interface IProductAttributeService
    {
        Task<ServiceResult<List<ProductAttributeDto>>> GetAllAsync(CancellationToken ct = default);
        Task<ServiceResult<ProductAttributeDto>> GetByIdAsync(int id, CancellationToken ct = default);
        Task<ServiceResult<ProductAttributeDto>> CreateAsync(CreateProductAttributeDto dto, CancellationToken ct = default);
        Task<ServiceResult<ProductAttributeDto>> UpdateAsync(UpdateProductAttributeDto dto, CancellationToken ct = default);
        Task<ServiceResult<string>> DeleteAsync(int id, CancellationToken ct = default);
    }
}