using BuildingBlocks.Results;
using Onpoint.Store.Application.DTOs.Coupon;

namespace Onpoint.Store.Application.Services.CouponServ
{
    public interface ICouponService
    {
        Task<ServiceResult<CouponDto>> CreateAsync(CreateCouponDto dto, CancellationToken ct = default);
        Task<ServiceResult<CouponDto>> UpdateAsync(int id, UpdateCouponDto dto, CancellationToken ct = default);
        Task<ServiceResult<bool>> DeleteAsync(int id, CancellationToken ct = default);
        Task<ServiceResult<CouponDto>> GetByIdAsync(int id, CancellationToken ct = default);
        Task<ServiceResult<IEnumerable<CouponDto>>> GetAllAsync(CancellationToken ct = default);
    }
}