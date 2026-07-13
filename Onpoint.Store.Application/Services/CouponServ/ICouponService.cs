using BuildingBlocks.Results;
using Onpoint.Store.Application.DTOs.Coupon;

namespace Onpoint.Store.Application.Services.CouponServ
{

    public interface ICouponService
    {
        Task<ServiceResult<CouponDto>> CreateAsync(CreateCouponDto dto);
        Task<ServiceResult<CouponDto>> UpdateAsync(int id, UpdateCouponDto dto);
        Task<ServiceResult<bool>> DeleteAsync(int id);
        Task<ServiceResult<CouponDto>> GetByIdAsync(int id);
        Task<ServiceResult<IEnumerable<CouponDto>>> GetAllAsync();
    }

}
