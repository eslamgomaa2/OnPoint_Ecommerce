using AutoMapper;
using BuildingBlocks.Results;
using FluentValidation;
using Onpoint.Store.Application.DTOs.Discount;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Repositories;

namespace Onpoint.Store.Application.Services.DiscountServ
{
    public class DiscountService : IDiscountService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ServiceResultHandler _resultHandler;
        private readonly IValidator<AddDiscountDto> _addValidator;
        private readonly IValidator<UpdateDiscountDto> _updateValidator;

        public DiscountService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ServiceResultHandler resultHandler,
            IValidator<AddDiscountDto> addValidator,
            IValidator<UpdateDiscountDto> updateValidator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _resultHandler = resultHandler;
            _addValidator = addValidator;
            _updateValidator = updateValidator;
        }
        public async Task<ServiceResult<DiscountDto>> UpdateDiscountAsync(int id, UpdateDiscountDto dto, CancellationToken ct = default)
        {
            await _updateValidator.ValidateAndThrowAsync(dto, cancellationToken: ct);

            var discount = await _unitOfWork.Discounts.GetByIdAsync(id, ct);
            if (discount is null)
                return _resultHandler.NotFound<DiscountDto>("Discount not found.");
            /*
                        var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId, ct);
                        if (product is null)
                            return _resultHandler.NotFound<DiscountDto>("Product not found.");

                        var hasOverlap = await _unitOfWork.Discounts.HasOverlappingDiscountAsync(
                            dto.ProductId, dto.StartDate, dto.EndDate, ct);

                        // exclude the current discount itself from the overlap check
                        if (hasOverlap && discount.ProductId == dto.ProductId)
                        {
                            var otherOverlap = await _unitOfWork.Discounts.HasOverlappingDiscountAsync(
                                dto.ProductId, dto.StartDate, dto.EndDate, ct);

                        }*/

            //discount.ProductId = dto.ProductId;
            discount.DiscountPercentage = dto.DiscountPercentage;
            discount.StartDate = dto.StartDate;
            discount.EndDate = dto.EndDate;
            discount.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Discounts.Update(discount);
            await _unitOfWork.SaveChangesAsync(ct);

            var resultDto = _mapper.Map<DiscountDto>(discount);
            //resultDto.ProductName = product.Name;

            return _resultHandler.Success(resultDto);
        }
        public async Task<ServiceResult<DiscountDto>> AddDiscountAsync(AddDiscountDto dto, CancellationToken ct = default)
        {
            await _addValidator.ValidateAndThrowAsync(dto, cancellationToken: ct);

            var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId, ct);
            if (product is null)
                return _resultHandler.NotFound<DiscountDto>("Product not found.");

            var discount = _mapper.Map<Discount>(dto);
            discount.IsActive = true;

            await _unitOfWork.Discounts.AddAsync(discount, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var resultDto = _mapper.Map<DiscountDto>(discount);
            resultDto.ProductName = product.Name;

            return _resultHandler.Created(resultDto);
        }

        public async Task<ServiceResult<List<DiscountDto>>> GetByProductAsync(int productId, CancellationToken ct = default)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId, ct);
            if (product is null)
                return _resultHandler.NotFound<List<DiscountDto>>("Product not found.");

            var discounts = await _unitOfWork.Discounts.GetDiscountsByProductAsync(productId, ct);

            var dtos = _mapper.Map<List<DiscountDto>>(discounts);
            dtos.ForEach(d => d.ProductName = product.Name);

            return _resultHandler.Success(dtos);
        }

        public async Task<ServiceResult<string>> DeactivateAsync(int discountId, CancellationToken ct = default)
        {
            var discount = await _unitOfWork.Discounts.GetByIdAsync(discountId, ct);
            if (discount is null)
                return _resultHandler.NotFound<string>("Discount not found.");

            discount.IsActive = false;
            discount.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Discounts.Update(discount);
            await _unitOfWork.SaveChangesAsync(ct);

            return _resultHandler.Success<string>("Discount deactivated successfully.");
        }

        public async Task<ServiceResult<PagedResult<DiscountDto>>> GetAllAsync(
      DiscountFilterRequest filter,
      CancellationToken ct = default)
        {
            var (items, totalCount) = await _unitOfWork.Discounts.GetAllPagedAsync(
                filter.ProductId,
                filter.SearchTerm,
                filter.IsActive,
                filter.PageNumber,
                filter.PageSize,
                ct);

            var dtos = _mapper.Map<List<DiscountDto>>(items);

            var pagedResult = PagedResult<DiscountDto>.Create(
                dtos,
                totalCount,
                filter.PageNumber,
                filter.PageSize);

            return _resultHandler.Success(pagedResult);
        }
    }
}