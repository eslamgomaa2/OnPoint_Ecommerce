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
        private readonly IValidator<UpdateCouponDto> _updateCouponValidator;
        private readonly IValidator<CreateCouponDto> _createCouponValidator;
        private readonly ServiceResultHandler _serviceResultHandler;

        public CouponService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ServiceResultHandler serviceResultHandler,
            IValidator<UpdateCouponDto> updateCouponValidator,
            IValidator<CreateCouponDto> createCouponValidator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _serviceResultHandler = serviceResultHandler;
            _updateCouponValidator = updateCouponValidator;
            _createCouponValidator = createCouponValidator;
        }

        public async Task<ServiceResult<CouponDto>> CreateAsync(CreateCouponDto dto)
        {
            await _createCouponValidator.ValidateAndThrowAsync(dto);

            var entity = _mapper.Map<Coupon>(dto);
            await _unitOfWork.Coupons.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            var couponDto = _mapper.Map<CouponDto>(entity);
            return _serviceResultHandler.Created<CouponDto>(couponDto);
        }

        public async Task<ServiceResult<CouponDto>> UpdateAsync(int id, UpdateCouponDto dto)
        {
            await _updateCouponValidator.ValidateAndThrowAsync(dto);

            var existingCoupon = await _unitOfWork.Coupons.GetByIdAsync(id);
            if (existingCoupon == null)
                return _serviceResultHandler.NotFound<CouponDto>("Coupon not found");

            _mapper.Map(dto, existingCoupon);
            _unitOfWork.Coupons.Update(existingCoupon);
            await _unitOfWork.SaveChangesAsync();

            var couponDto = _mapper.Map<CouponDto>(existingCoupon);
            return _serviceResultHandler.Success<CouponDto>(couponDto);
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int id)
        {
            var existingCoupon = await _unitOfWork.Coupons.GetByIdAsync(id);
            if (existingCoupon == null)
                return _serviceResultHandler.NotFound<bool>("Coupon not found");


            _unitOfWork.Coupons.Remove(existingCoupon);
            await _unitOfWork.SaveChangesAsync();

            return _serviceResultHandler.Deleted<bool>();
        }

        public async Task<ServiceResult<CouponDto>> GetByIdAsync(int id)
        {
            var coupon = await _unitOfWork.Coupons.GetByIdAsync(id);
            if (coupon == null)
                return _serviceResultHandler.NotFound<CouponDto>("Coupon not found");

            var couponDto = _mapper.Map<CouponDto>(coupon);
            return _serviceResultHandler.Success<CouponDto>(couponDto);
        }

        public async Task<ServiceResult<IEnumerable<CouponDto>>> GetAllAsync()
        {
            var coupons = await _unitOfWork.Coupons.GetAllAsync();
            var couponsDto = _mapper.Map<IEnumerable<CouponDto>>(coupons);
            return _serviceResultHandler.Success<IEnumerable<CouponDto>>(couponsDto);
        }
    }
}