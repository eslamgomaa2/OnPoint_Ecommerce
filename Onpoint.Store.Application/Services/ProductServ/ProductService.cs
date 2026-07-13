using AutoMapper;
using BuildingBlocks.Results;
using FluentValidation;
using Onpoint.Store.Application.DTOs.Product;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Repositories;

namespace Onpoint.Store.Application.Services.ProductServ
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ServiceResultHandler _resultHandler;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateProductDto> _createValidator;
        private readonly IValidator<UpdateProductDto> _updateValidator;

        public ProductService(
            IUnitOfWork unitOfWork,
            ServiceResultHandler resultHandler,
            IMapper mapper,
            IValidator<CreateProductDto> createValidator,
            IValidator<UpdateProductDto> updateValidator)
        {
            _unitOfWork = unitOfWork;
            _resultHandler = resultHandler;
            _mapper = mapper;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<ServiceResult<PagedResult<ProductDto>>> GetFilteredPagedAsync(PaginationRequest request, int? categoryId = null, string? searchTerm = null, CancellationToken ct = default)
        {
            var (items, totalCount) = await _unitOfWork.Products.GetFilteredPagedAsync(categoryId, searchTerm, request.PageNumber, request.PageSize, ct);

            var dtoItems = _mapper.Map<IReadOnlyList<ProductDto>>(items);
            var pagedResult = PagedResult<ProductDto>.Create(dtoItems, totalCount, request.PageNumber, request.PageSize);

            return _resultHandler.Success(pagedResult);
        }

        public async Task<ServiceResult<ProductDetailDto>> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var product = await _unitOfWork.Products.GetWithDetailsAsync(id, ct);

            if (product is null)
                return _resultHandler.NotFound<ProductDetailDto>("Product not found");

            return _resultHandler.Success(_mapper.Map<ProductDetailDto>(product));
        }

        public async Task<ServiceResult<ProductDto>> CreateAsync(CreateProductDto dto, CancellationToken ct = default)
        {
            var validationResult = await _createValidator.ValidateAsync(dto, ct);
            if (!validationResult.IsValid)
                throw new FluentValidation.ValidationException(validationResult.Errors);


            var product = _mapper.Map<Product>(dto);

            if (dto.Images != null && dto.Images.Any())
            {
                foreach (var imageDto in dto.Images)
                {
                    product.Images.Add(new ProductImage
                    {
                        ImageUrl = imageDto.ImageUrl,
                        IsPrimary = imageDto.IsPrimary
                    });
                }
            }


            if (dto.Discount != null)
            {
                product.Discounts.Add(new Discount
                {
                    DiscountPercentage = dto.Discount.DiscountPercentage,
                    StartDate = dto.Discount.StartDate,
                    EndDate = dto.Discount.EndDate,
                    IsActive = true
                });
            }


            await _unitOfWork.Products.AddAsync(product, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return _resultHandler.Created(_mapper.Map<ProductDto>(product));
        }

        public async Task<ServiceResult<ProductDto>> UpdateAsync(UpdateProductDto dto, CancellationToken ct = default)
        {

            var validationResult = await _updateValidator.ValidateAsync(dto, ct);
            if (!validationResult.IsValid)
                throw new FluentValidation.ValidationException(validationResult.Errors);


            var existingProduct = await _unitOfWork.Products.GetWithDetailsAsync(dto.Id, ct);
            if (existingProduct is null)
                return _resultHandler.NotFound<ProductDto>("Product not found to update");


            _mapper.Map(dto, existingProduct);
            existingProduct.UpdatedAt = DateTime.UtcNow;

            existingProduct.Images.Clear();
            existingProduct.Discounts.Clear();

            if (dto.Images != null && dto.Images.Any())
            {
                var newImages = _mapper.Map<List<ProductImage>>(dto.Images);
                existingProduct.Images = newImages;
            }

            if (dto.Discount != null)
            {
                existingProduct.Discounts = new List<Discount>
                {
                    new Discount
                    {
                        DiscountPercentage = dto.Discount.DiscountPercentage,
                        StartDate = dto.Discount.StartDate,
                        EndDate = dto.Discount.EndDate,
                        IsActive = true
                    }
                };
            }

            _unitOfWork.Products.Update(existingProduct);
            await _unitOfWork.SaveChangesAsync(ct);

            return _resultHandler.Success(_mapper.Map<ProductDto>(existingProduct));
        }

        public async Task<ServiceResult<string>> DeleteAsync(int id, CancellationToken ct = default)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id, ct);
            if (product is null)
                return _resultHandler.NotFound<string>("Product not found to delete");


            product.IsDeleted = true;
            product.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Products.Update(product);
            await _unitOfWork.SaveChangesAsync(ct);

            return _resultHandler.Deleted<string>();
        }
    }
}
