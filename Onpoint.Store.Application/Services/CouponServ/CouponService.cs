using AutoMapper;
using BuildingBlocks.Results;
using FluentValidation;
using Onpoint.Store.Application.DTOs.Coupon;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Repositories;

namespace Onpoint.Store.Application.Services.CouponServ
{
    public class CouponService : ICouponService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ServiceResultHandler _serviceResultHandler;
        private readonly IValidator<CreateCouponDto> _createCouponValidator;
        private readonly IValidator<UpdateCouponDto> _updateCouponValidator;

        public CouponService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ServiceResultHandler serviceResultHandler,
            IValidator<CreateCouponDto> createCouponValidator,
            IValidator<UpdateCouponDto> updateCouponValidator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _serviceResultHandler = serviceResultHandler;
            _createCouponValidator = createCouponValidator;
            _updateCouponValidator = updateCouponValidator;
        }

        public async Task<ServiceResult<CouponDto>> CreateAsync(CreateCouponDto dto, CancellationToken ct = default)
        {
            await _createCouponValidator.ValidateAndThrowAsync(dto, cancellationToken: ct);

            var entity = _mapper.Map<Coupon>(dto);
            await _unitOfWork.Coupons.AddAsync(entity, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var couponDto = _mapper.Map<CouponDto>(entity);
            return _serviceResultHandler.Created(couponDto);
        }

        public async Task<ServiceResult<CouponDto>> UpdateAsync(int id, UpdateCouponDto dto, CancellationToken ct = default)
        {
            await _updateCouponValidator.ValidateAndThrowAsync(dto, cancellationToken: ct);

            var existingCoupon = await _unitOfWork.Coupons.GetByIdAsync(id, ct);
            if (existingCoupon == null)
                return _serviceResultHandler.NotFound<CouponDto>("Coupon not found");

            _mapper.Map(dto, existingCoupon);
            _unitOfWork.Coupons.Update(existingCoupon);
            await _unitOfWork.SaveChangesAsync(ct);

            var couponDto = _mapper.Map<CouponDto>(existingCoupon);
            return _serviceResultHandler.Success(couponDto);
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int id, CancellationToken ct = default)
        {
            var existingCoupon = await _unitOfWork.Coupons.GetByIdAsync(id, ct);
            if (existingCoupon == null)
                return _serviceResultHandler.NotFound<bool>("Coupon not found");

            _unitOfWork.Coupons.Remove(existingCoupon);
            await _unitOfWork.SaveChangesAsync(ct);

            return _serviceResultHandler.Deleted<bool>();
        }

        public async Task<ServiceResult<CouponDto>> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var coupon = await _unitOfWork.Coupons.GetByIdAsync(id, ct);
            if (coupon == null)
                return _serviceResultHandler.NotFound<CouponDto>("Coupon not found");

            var couponDto = _mapper.Map<CouponDto>(coupon);
            return _serviceResultHandler.Success(couponDto);
        }

        public async Task<ServiceResult<IEnumerable<CouponDto>>> GetAllAsync(CancellationToken ct = default)
        {
            var coupons = await _unitOfWork.Coupons.GetAllAsync(ct: ct);
            var couponsDto = _mapper.Map<IEnumerable<CouponDto>>(coupons);
            return _serviceResultHandler.Success(couponsDto);
        }
    }
}