using AutoMapper;
using BuildingBlocks.Results;
using FluentValidation;
using Onpoint.Store.Application.DTOs.Brand;
using Onpoint.Store.Application.DTOs.Product;
using Onpoint.Store.Application.Helpers;
using Onpoint.Store.Domin.Repositories;

namespace Onpoint.Store.Application.Services.Brand
{
    public class BrandService : IBrandService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateBrandDto> _createValidator;
        private readonly IValidator<UpdateBrandDto> _updateValidator;
        private readonly ServiceResultHandler _resultHandler;

        public BrandService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<CreateBrandDto> createValidator,
            IValidator<UpdateBrandDto> updateValidator,
            ServiceResultHandler resultHandler)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _resultHandler = resultHandler;
        }

        public async Task<ServiceResult<IEnumerable<ProductDto>>> GetProductsByBrandIdAsync(int brandId, CancellationToken ct = default)
        {
            var brand = await _unitOfWork.Brands.GetByIdAsync(brandId, ct);
            if (brand == null)
                return _resultHandler.NotFound<IEnumerable<ProductDto>>("Brand not found");

            var products = await _unitOfWork.Brands.GetProductsByBrandIdAsync(brandId, ct);
            var dtos = _mapper.Map<IEnumerable<ProductDto>>(products);

            return _resultHandler.Success(dtos);
        }

        public async Task<ServiceResult<IEnumerable<BrandDto>>> GetAllAsync(CancellationToken ct = default)
        {
            var brands = await _unitOfWork.Brands.GetAllAsync(ct);
            var dtos = _mapper.Map<IEnumerable<BrandDto>>(brands);
            return _resultHandler.Success(dtos);
        }

        public async Task<ServiceResult<BrandDto>> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var brand = await _unitOfWork.Brands.GetByIdAsync(id, ct);
            if (brand == null)
                return _resultHandler.NotFound<BrandDto>("Brand not found");

            var dto = _mapper.Map<BrandDto>(brand);
            return _resultHandler.Success(dto);
        }

        public async Task<ServiceResult<(IReadOnlyList<BrandDto> Items, int TotalCount)>> GetFilteredPagedAsync(
            string? searchTerm, bool? isActive, int pageNumber, int pageSize, CancellationToken ct = default)
        {
            var (items, totalCount) = await _unitOfWork.Brands.GetFilteredPagedAsync(searchTerm, isActive, pageNumber, pageSize, ct);
            var dtos = _mapper.Map<IReadOnlyList<BrandDto>>(items);

            return _resultHandler.Success((dtos, totalCount));
        }

        public async Task<ServiceResult<BrandDto>> CreateAsync(CreateBrandDto dto, CancellationToken ct = default)
        {
            await _createValidator.ValidateAndThrowAsync(dto, cancellationToken: ct);

            var brand = _mapper.Map<Domin.Entities.Brand>(dto);
            brand.Slug = await GenerateUniqueSlugAsync(dto.Name, null, ct);

            await _unitOfWork.Brands.AddAsync(brand, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var resultDto = _mapper.Map<BrandDto>(brand);
            return _resultHandler.Created(resultDto);
        }

        public async Task<ServiceResult<BrandDto>> UpdateAsync(int id, UpdateBrandDto dto, CancellationToken ct = default)
        {
            await _updateValidator.ValidateAndThrowAsync(dto, cancellationToken: ct);

            var brand = await _unitOfWork.Brands.GetByIdAsync(id, ct);
            if (brand == null)
                return _resultHandler.NotFound<BrandDto>("Brand not found");

            if (!string.Equals(brand.Name, dto.Name, StringComparison.OrdinalIgnoreCase))
            {
                brand.Slug = await GenerateUniqueSlugAsync(dto.Name, id, ct);
            }

            _mapper.Map(dto, brand);

            _unitOfWork.Brands.Update(brand);
            await _unitOfWork.SaveChangesAsync(ct);

            var resultDto = _mapper.Map<BrandDto>(brand);
            return _resultHandler.Success(resultDto);
        }

        public async Task<ServiceResult<bool>> ToggleActiveAsync(int id, CancellationToken ct = default)
        {
            var brand = await _unitOfWork.Brands.GetByIdAsync(id, ct);
            if (brand == null)
                return _resultHandler.NotFound<bool>("Brand not found");

            brand.IsActive = !brand.IsActive;
            _unitOfWork.Brands.Update(brand);
            await _unitOfWork.SaveChangesAsync(ct);

            return _resultHandler.Success(true);
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int id, CancellationToken ct = default)
        {
            var brand = await _unitOfWork.Brands.GetWithProductsAsync(id, ct);
            if (brand == null)
                return _resultHandler.NotFound<bool>("Brand not found");

            if (brand.Products.Any())
                return _resultHandler.BadRequest<bool>("Cannot delete a brand that has products linked to it");

            _unitOfWork.Brands.Remove(brand);
            await _unitOfWork.SaveChangesAsync(ct);

            return _resultHandler.Success(true);
        }

        private async Task<string> GenerateUniqueSlugAsync(string name, int? excludeBrandId, CancellationToken ct = default)
        {
            var baseSlug = SlugHelper.GenerateSlug(name);
            var slug = baseSlug;
            var counter = 2;

            while (await _unitOfWork.Brands.SlugExistsAsync(slug, excludeBrandId, ct))
            {
                slug = SlugHelper.AppendSuffix(baseSlug, counter);
                counter++;
            }

            return slug;
        }
    }
}