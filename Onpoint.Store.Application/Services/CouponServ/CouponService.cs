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
        private readonly ServiceResultHandler _resultHandler;

        public CouponService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ServiceResultHandler serviceResultHandler,
            IValidator<CreateCouponDto> createCouponValidator,
            IValidator<UpdateCouponDto> updateCouponValidator,
            ServiceResultHandler resultHandler)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _serviceResultHandler = serviceResultHandler;
            _createCouponValidator = createCouponValidator;
            _updateCouponValidator = updateCouponValidator;
            _resultHandler = resultHandler;
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

        public async Task<ServiceResult<PagedResult<CouponDto>>> GetAllAsync(
     CouponFilterRequest filter,
     CancellationToken ct = default)
        {
            var (items, totalCount) = await _unitOfWork.Coupons.GetAllPagedAsync(
                filter.SearchTerm,
                filter.IsActive,
                filter.DiscountType,
                filter.PageNumber,
                filter.PageSize,
                ct);

            var dtos = _mapper.Map<List<CouponDto>>(items);

            var pagedResult = PagedResult<CouponDto>.Create(
                dtos,
                totalCount,
                filter.PageNumber,
                filter.PageSize);

            return _resultHandler.Success(pagedResult);
        }

    }
}