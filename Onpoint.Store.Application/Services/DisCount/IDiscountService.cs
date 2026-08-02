using BuildingBlocks.Results;
using Onpoint.Store.Application.DTOs.Discount;

namespace Onpoint.Store.Application.Services.DiscountServ
{
    public interface IDiscountService
    {
        Task<ServiceResult<DiscountDto>> AddDiscountAsync(AddDiscountDto dto, CancellationToken ct = default);
        Task<ServiceResult<List<DiscountDto>>> GetByProductAsync(int productId, CancellationToken ct = default);
        Task<ServiceResult<DiscountDto>> UpdateDiscountAsync(int id, UpdateDiscountDto dto, CancellationToken ct = default);
        Task<ServiceResult<string>> DeactivateAsync(int discountId, CancellationToken ct = default);
        Task<ServiceResult<PagedResult<DiscountDto>>> GetAllAsync(DiscountFilterRequest filter, CancellationToken ct = default);
    }
}