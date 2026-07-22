
using AutoMapper;
using BuildingBlocks.Results;
using Onpoint.Store.Application.DTOs.Product;
using Onpoint.Store.Application.Interfaces;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Repositories;

namespace Onpoint.Store.Application.Services.ShippingServ
{
    public class ShippingService : IShippingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ServiceResultHandler _resultHandler;

        public ShippingService(IUnitOfWork unitOfWork, IMapper mapper, ServiceResultHandler resultHandler)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _resultHandler = resultHandler;
        }

        public async Task<ServiceResult<ProductShippingDto>> GetByProductIdAsync(int productId, CancellationToken ct = default)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId, ct);
            if (product is null)
                return _resultHandler.NotFound<ProductShippingDto>("Product not found");

            if (product.Shipping is null)
                return _resultHandler.NotFound<ProductShippingDto>("Shipping data not found for this product");

            var dto = _mapper.Map<ProductShippingDto>(product.Shipping);
            return _resultHandler.Success(dto);
        }

        public async Task<ServiceResult<ProductShippingDto>> CreateAsync(int productId, CreateProductShippingDto dto, CancellationToken ct = default)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId, ct);
            if (product is null)
                return _resultHandler.NotFound<ProductShippingDto>("Product not found");

            if (product.Shipping is not null)
                return _resultHandler.BadRequest<ProductShippingDto>("Product already has shipping data. Use Update instead.");

            var shipping = _mapper.Map<ProductShipping>(dto);
            shipping.ProductId = productId;

            product.Shipping = shipping;
            _unitOfWork.Products.Update(product);
            await _unitOfWork.SaveChangesAsync(ct);

            var resultDto = _mapper.Map<ProductShippingDto>(shipping);
            return _resultHandler.Created(resultDto);
        }

        public async Task<ServiceResult<ProductShippingDto>> UpdateAsync(int productId, UpdateProductShippingDto dto, CancellationToken ct = default)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId, ct);
            if (product is null)
                return _resultHandler.NotFound<ProductShippingDto>("Product not found");

            if (product.Shipping is null)
                return _resultHandler.NotFound<ProductShippingDto>("Shipping data not found for this product");

            product.Shipping.WeightKg = dto.WeightKg;
            product.Shipping.LengthCm = dto.LengthCm;
            product.Shipping.WidthCm = dto.WidthCm;
            product.Shipping.HeightCm = dto.HeightCm;
            product.Shipping.IsFragile = dto.IsFragile;
            product.Shipping.IsHazardous = dto.IsHazardous;
            product.Shipping.ShippingClass = dto.ShippingClass;

            _unitOfWork.Products.Update(product);
            await _unitOfWork.SaveChangesAsync(ct);

            var resultDto = _mapper.Map<ProductShippingDto>(product.Shipping);
            return _resultHandler.Success(resultDto);
        }

        public async Task<ServiceResult<string>> DeleteAsync(int productId, CancellationToken ct = default)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId, ct);
            if (product is null)
                return _resultHandler.NotFound<string>("Product not found");

            if (product.Shipping is null)
                return _resultHandler.NotFound<string>("Shipping data not found for this product");

            product.Shipping.IsDeleted = true;
            product.Shipping.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Products.Update(product);
            await _unitOfWork.SaveChangesAsync(ct);

            return _resultHandler.Deleted<string>();
        }
    }
}