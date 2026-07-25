using BuildingBlocks.Results;
using Onpoint.Store.Application.DTOs.Product;
using Onpoint.Store.Application.DTOs.ProductVariant;

namespace Onpoint.Store.Application.Services.ProductVariantServ
{
    public interface IProductVariantService
    {
        Task<ServiceResult<ProductVariantDto>> AddVariantAsync(int productId, CreateProductVariantDto dto, CancellationToken ct = default);
        Task<ServiceResult<IReadOnlyList<ProductVariantDto>>> GetVariantsAsync(int productId, CancellationToken ct = default);
        Task<ServiceResult<ProductVariantDto>> GetVariantByIdAsync(int productId, int variantId, CancellationToken ct = default);
        Task<ServiceResult<ProductVariantDto>> UpdateVariantAsync(int productId, int variantId, UpdateProductVariantDto dto, CancellationToken ct = default);
        Task<ServiceResult<string>> DeactivateVariantAsync(int productId, int variantId, CancellationToken ct = default);
        Task<ServiceResult<string>> ActivateVariantAsync(int productId, int variantId, CancellationToken ct = default);
    }
}