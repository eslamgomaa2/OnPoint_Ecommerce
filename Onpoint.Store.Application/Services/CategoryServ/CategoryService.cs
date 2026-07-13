using AutoMapper;
using BuildingBlocks.Results;
using FluentValidation;
using Onpoint.Store.Application.DTOs.Category;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Repositories;

namespace Onpoint.Store.Application.Services.CategoryServ
{

    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ServiceResultHandler _resultHandler;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateCategoryDto> _createValidator;
        private readonly IValidator<UpdateCategoryDto> _updateValidator;

        public CategoryService(
            IUnitOfWork unitOfWork,
            ServiceResultHandler resultHandler,
            IMapper mapper,
            IValidator<CreateCategoryDto> createValidator,
            IValidator<UpdateCategoryDto> updateValidator)
        {
            _unitOfWork = unitOfWork;
            _resultHandler = resultHandler;
            _mapper = mapper;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<ServiceResult<IReadOnlyList<CategoryDto>>> GetAllAsync(CancellationToken ct = default)
        {
            var categories = await _unitOfWork.Categories.GetAllAsync(ct);
            return _resultHandler.Success(_mapper.Map<IReadOnlyList<CategoryDto>>(categories));
        }

        public async Task<ServiceResult<PagedResult<CategoryDto>>> GetPagedAsync(PaginationRequest request, CancellationToken ct = default)
        {
            var (items, totalCount) = await _unitOfWork.Categories.GetPagedAsync(
                request.PageNumber,
                request.PageSize,
                orderBy: q => q.OrderBy(c => c.Name),
                ct: ct
            );

            var pagedResult = PagedResult<CategoryDto>.Create(
                _mapper.Map<IReadOnlyList<CategoryDto>>(items),
                totalCount,
                request.PageNumber,
                request.PageSize);

            return _resultHandler.Success(pagedResult);
        }

        public async Task<ServiceResult<CategoryDto>> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id, ct);
            if (category is null)
                return _resultHandler.NotFound<CategoryDto>("Category not found");

            return _resultHandler.Success(_mapper.Map<CategoryDto>(category));
        }

        public async Task<ServiceResult<CategoryDto>> CreateAsync(CreateCategoryDto dto, CancellationToken ct = default)
        {
            var validationResult = await _createValidator.ValidateAsync(dto, ct);
            if (!validationResult.IsValid)
                throw new FluentValidation.ValidationException(validationResult.Errors);


            var category = _mapper.Map<Category>(dto);


            await _unitOfWork.Categories.AddAsync(category, ct);
            await _unitOfWork.SaveChangesAsync(ct);


            return _resultHandler.Created(_mapper.Map<CategoryDto>(category));
        }

        public async Task<ServiceResult<CategoryDto>> UpdateAsync(UpdateCategoryDto dto, CancellationToken ct = default)
        {

            var validationResult = await _updateValidator.ValidateAsync(dto, ct);
            if (!validationResult.IsValid)
                throw new FluentValidation.ValidationException(validationResult.Errors);


            var existingCategory = await _unitOfWork.Categories.GetByIdAsync(dto.Id, ct);
            if (existingCategory is null)
                return _resultHandler.NotFound<CategoryDto>("Category not found to update");

            _mapper.Map(dto, existingCategory);
            existingCategory.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Categories.Update(existingCategory);
            await _unitOfWork.SaveChangesAsync(ct);

            return _resultHandler.Success(_mapper.Map<CategoryDto>(existingCategory));
        }

        public async Task<ServiceResult<string>> DeleteAsync(int id, CancellationToken ct = default)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id, ct);
            if (category is null)
                return _resultHandler.NotFound<string>("Category not found to delete");


            category.IsDeleted = true;
            category.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Categories.Update(category);
            await _unitOfWork.SaveChangesAsync(ct);

            return _resultHandler.Deleted<string>();
        }
    }
}
