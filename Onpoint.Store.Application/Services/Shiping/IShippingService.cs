
using BuildingBlocks.Results;
using Onpoint.Store.Application.DTOs.Product;

namespace Onpoint.Store.Application.Interfaces
{
    public interface IShippingService
    {
        Task<ServiceResult<ProductShippingDto>> GetByProductIdAsync(int productId, CancellationToken ct = default);
        Task<ServiceResult<ProductShippingDto>> CreateAsync(int productId, CreateProductShippingDto dto, CancellationToken ct = default);
        Task<ServiceResult<ProductShippingDto>> UpdateAsync(int productId, UpdateProductShippingDto dto, CancellationToken ct = default);
        Task<ServiceResult<string>> DeleteAsync(int productId, CancellationToken ct = default);
    }
}