using BuildingBlocks.Results;
using Onpoint.Store.Application.DTOs.Product;
using Onpoint.Store.Application.DTOs.ProductVariant;

namespace Onpoint.Store.Application.Services.ProductVariantServ
{
    public interface IProductVariantService
    {
        Task<ServiceResult<List<ProductVariantDto>>> GetAllVariantsAsync(ProductVariantFilterRequestDto filter, CancellationToken ct = default);
        Task<ServiceResult<List<ProductVariantDto>>> GetVariantsByProductIdAsync(int productId, CancellationToken ct = default);
        Task<ServiceResult<ProductVariantDto>> GetVariantByIdAsync(int id, CancellationToken ct = default);
        Task<ServiceResult<ProductVariantDto>> AddVariantAsync(int productId, CreateProductVariantDto dto, CancellationToken ct = default);
        Task<ServiceResult<ProductVariantDto>> UpdateVariantAsync(int productId, int variantId, UpdateProductVariantDto dto, CancellationToken ct = default);
        Task<ServiceResult<string>> DeleteVariantAsync(int productId, int variantId, CancellationToken ct = default);
        Task<ServiceResult<string>> ToggleVariantStatusAsync(int productId, int variantId, CancellationToken ct = default);
    }
}