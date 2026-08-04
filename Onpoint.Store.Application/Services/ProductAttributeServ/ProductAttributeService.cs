using AutoMapper;
using BuildingBlocks.Common;
using BuildingBlocks.Results;
using FluentValidation;
using Onpoint.Store.Application.DTOs.ProductAttribute;
using Onpoint.Store.Application.Mapping;
using Onpoint.Store.Domin.Repositories;

namespace Onpoint.Store.Application.Services.ProductAttributeServ
{
    public class ProductAttributeService : IProductAttributeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ServiceResultHandler _resultHandler;
        private readonly IValidator<CreateProductAttributeDto> _createValidator;
        private readonly IValidator<UpdateProductAttributeDto> _updateValidator;
        private readonly ICurrentLanguage _currentLanguage;

        public ProductAttributeService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ServiceResultHandler resultHandler,
            IValidator<CreateProductAttributeDto> createValidator,
            IValidator<UpdateProductAttributeDto> updateValidator,
            ICurrentLanguage currentLanguage)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _resultHandler = resultHandler;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _currentLanguage = currentLanguage;
        }

        public async Task<ServiceResult<PagedResult<ProductAttributeDto>>> GetAllWithCategoriesAsync(
     ProductAttributeFilter filter,
     CancellationToken ct = default)
        {
            var (items, totalCount) = await _unitOfWork.ProductAttributes
                .GetAllWithCategoriesAsync(
                    filter.PageNumber,
                    filter.PageSize,
                    filter.SearchTerm,
                    filter.IsActive,
                    filter.ValueType,
                    ct);

            var lang = _currentLanguage.Lang;

            var dtos = items.Select(a => new ProductAttributeDto
            {
                Id = a.Id,
                Name = LocalizationHelper.Pick(a.Name, a.NameEn, lang),
                Key = a.Key,
                IsActive = a.IsActive,
                ValueType = a.ValueType,
            }).ToList();
            var pagedResult = PagedResult<ProductAttributeDto>.Create(dtos, totalCount, filter.PageNumber, filter.PageSize);

            return _resultHandler.Success(pagedResult);
        }

        public async Task<ServiceResult<ProductAttributeDto>> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var attribute = await _unitOfWork.ProductAttributes.GetByIdWithCategoriesAsync(id, ct);
            if (attribute is null)
                return _resultHandler.NotFound<ProductAttributeDto>("Attribute not found.");

            return _resultHandler.Success<ProductAttributeDto>(
    _mapper.Map<ProductAttributeDto>(attribute, opts => opts.Items["lang"] = _currentLanguage.Lang));
        }

        public async Task<ServiceResult<ProductAttributeDto>> CreateAsync(CreateProductAttributeDto dto, CancellationToken ct = default)
        {
            var validationResult = await _createValidator.ValidateAsync(dto, ct);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var keyExists = await _unitOfWork.ProductAttributes.KeyExistsAsync(dto.Key, null, ct);
            if (keyExists)
                return _resultHandler.BadRequest<ProductAttributeDto>($"An attribute with the key '{dto.Key}' already exists.");

            var attribute = _mapper.Map<Domin.Entities.ProductAttribute>(dto);



            await _unitOfWork.ProductAttributes.AddAsync(attribute, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var saved = await _unitOfWork.ProductAttributes.GetByIdWithCategoriesAsync(attribute.Id, ct);

            return _resultHandler.Created<ProductAttributeDto>(
    _mapper.Map<ProductAttributeDto>(saved, opts => opts.Items["lang"] = _currentLanguage.Lang));
        }

        public async Task<ServiceResult<ProductAttributeDto>> UpdateAsync(int id, UpdateProductAttributeDto dto, CancellationToken ct = default)
        {
            var validationResult = await _updateValidator.ValidateAsync(dto, ct);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var existing = await _unitOfWork.ProductAttributes.GetByIdWithCategoriesAsync(id, ct);
            if (existing is null)
                return _resultHandler.NotFound<ProductAttributeDto>("Attribute not found.");

            existing.Name = dto.Name;
            existing.Key = dto.Key;
            existing.ValueType = dto.ValueType;



            existing.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.ProductAttributes.Update(existing);
            await _unitOfWork.SaveChangesAsync(ct);
            return _resultHandler.Success<ProductAttributeDto>(
    _mapper.Map<ProductAttributeDto>(existing, opts => opts.Items["lang"] = _currentLanguage.Lang));
        }

        public async Task<ServiceResult<string>> DeleteAsync(int id, CancellationToken ct = default)
        {
            var attribute = await _unitOfWork.ProductAttributes.GetByIdAsync(id, ct);
            if (attribute is null)
                return _resultHandler.NotFound<string>("Attribute not found.");

            var attributeWithUsage = await _unitOfWork.ProductAttributes.GetByIdWithCategoriesAsync(id, ct);
            var isInUse = attributeWithUsage!.ProductValues.Any() || attributeWithUsage.VariantValues.Any();

            if (isInUse)
                return _resultHandler.BadRequest<string>(
                    "Cannot delete this attribute because it is used by existing products or variants.");


            _unitOfWork.ProductAttributes.Remove(attribute);
            await _unitOfWork.SaveChangesAsync(ct);

            return _resultHandler.Deleted<string>();
        }
    }
}