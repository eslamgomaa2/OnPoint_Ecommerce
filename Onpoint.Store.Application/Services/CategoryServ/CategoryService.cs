using AutoMapper;
using BuildingBlocks.Common;
using BuildingBlocks.Results;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Onpoint.Store.Application.DTOs.Category;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Repositories;
using System.Linq.Expressions;

namespace Onpoint.Store.Application.Services.CategoryServ
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ServiceResultHandler _resultHandler;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateCategoryDto> _createValidator;
        private readonly IValidator<UpdateCategoryDto> _updateValidator;
        private readonly ICurrentLanguage _currentLanguage;

        public CategoryService(
            IUnitOfWork unitOfWork,
            ServiceResultHandler resultHandler,
            IMapper mapper,
            IValidator<CreateCategoryDto> createValidator,
            IValidator<UpdateCategoryDto> updateValidator,
            ICurrentLanguage currentLanguage)
        {
            _unitOfWork = unitOfWork;
            _resultHandler = resultHandler;
            _mapper = mapper;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _currentLanguage = currentLanguage;
        }

        public async Task<ServiceResult<IReadOnlyList<CategoryDto>>> GetAllAsync(CancellationToken ct = default)
        {
            var categories = await _unitOfWork.Categories.GetAllAsync(
                include: q => q.Include(c => c.Products),
                ct: ct);

            return _resultHandler.Success(_mapper.Map<IReadOnlyList<CategoryDto>>(categories, opts => opts.Items["lang"] = _currentLanguage.Lang));
        }

        public async Task<ServiceResult<PagedResult<CategoryDto>>> GetPagedAsync(PaginationRequest request, CancellationToken ct = default)
        {
            Expression<Func<Category, bool>>? predicate = null;

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim();
                predicate = c => c.Name.Contains(term);
            }

            if (request.IsActive.HasValue)
            {
                Expression<Func<Category, bool>> statusPredicate = c => c.IsActive == request.IsActive.Value;
                predicate = predicate is null ? statusPredicate : CombineAnd(predicate, statusPredicate);
            }

            var (items, totalCount) = await _unitOfWork.Categories.GetPagedAsync(
                request.PageNumber,
                request.PageSize,
                predicate: predicate,
                include: q => q.Include(c => c.Products),
                orderBy: q => q.OrderBy(c => c.Name),
                ct: ct
            );

            var pagedResult = PagedResult<CategoryDto>.Create(
               _mapper.Map<IReadOnlyList<CategoryDto>>(items, opts => opts.Items["lang"] = _currentLanguage.Lang),
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

            return _resultHandler.Success(_mapper.Map<CategoryDto>(category, opts => opts.Items["lang"] = _currentLanguage.Lang));
        }

        public async Task<ServiceResult<CategoryDto>> CreateAsync(CreateCategoryDto dto, CancellationToken ct = default)
        {
            await _createValidator.ValidateAndThrowAsync(dto, cancellationToken: ct);

            var category = _mapper.Map<Category>(dto);

            await _unitOfWork.Categories.AddAsync(category, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return _resultHandler.Created(_mapper.Map<CategoryDto>(category, opts => opts.Items["lang"] = _currentLanguage.Lang));
        }

        public async Task<ServiceResult<CategoryDto>> UpdateAsync(int id, UpdateCategoryDto dto, CancellationToken ct = default)
        {
            await _updateValidator.ValidateAndThrowAsync(dto, cancellationToken: ct);

            var existingCategory = await _unitOfWork.Categories.GetByIdAsync(id, ct);
            if (existingCategory is null)
                return _resultHandler.NotFound<CategoryDto>("Category not found to update");

            var nameExists = await _unitOfWork.Categories.GetCategoryByName(dto.Name, ct);
            if (nameExists != null && nameExists.Id != id)
                return _resultHandler.BadRequest<CategoryDto>("A category with this name already exists.");

            _mapper.Map(dto, existingCategory);
            existingCategory.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Categories.Update(existingCategory);

            try
            {
                await _unitOfWork.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex)
            {
                return _resultHandler.BadRequest<CategoryDto>("A category with this name already exists.");
            }

            return _resultHandler.Success(_mapper.Map<CategoryDto>(existingCategory, opts => opts.Items["lang"] = _currentLanguage.Lang));
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

        private static Expression<Func<T, bool>> CombineAnd<T>(
            Expression<Func<T, bool>> left,
            Expression<Func<T, bool>> right)
        {
            var param = Expression.Parameter(typeof(T));
            var body = Expression.AndAlso(
                Expression.Invoke(left, param),
                Expression.Invoke(right, param));

            return Expression.Lambda<Func<T, bool>>(body, param);
        }
    }
}