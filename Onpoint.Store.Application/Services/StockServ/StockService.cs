using AutoMapper;
using BuildingBlocks.Results;
using FluentValidation;
using Onpoint.Store.Application.DTOs.Stock;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Enums;
using Onpoint.Store.Domin.Repositories;
using System.Data;

namespace Onpoint.Store.Application.Services.StockServ
{
    public class StockService : IStockService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ServiceResultHandler _resultHandler;
        private readonly IValidator<InitializeStockDto> _initializeValidator;
        private readonly IValidator<AdjustStockDto> _adjustValidator;
        private readonly IValidator<TransferStockDto> _transferValidator;

        public StockService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ServiceResultHandler resultHandler,
            IValidator<InitializeStockDto> initializeValidator,
            IValidator<AdjustStockDto> adjustValidator,
            IValidator<TransferStockDto> transferValidator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _resultHandler = resultHandler;
            _initializeValidator = initializeValidator;
            _adjustValidator = adjustValidator;
            _transferValidator = transferValidator;
        }
        public async Task<ServiceResult<int>> GetLowStockCountAsync(int? branchId = null, CancellationToken ct = default)
        {
            var count = await _unitOfWork.Stocks.GetLowStockCountAsync(branchId, ct);
            return _resultHandler.Success(count);
        }

        public async Task<ServiceResult<int>> GetInStockCountAsync(int? branchId = null, CancellationToken ct = default)
        {
            var count = await _unitOfWork.Stocks.GetInStockCountAsync(branchId, ct);
            return _resultHandler.Success(count);
        }

        public async Task<ServiceResult<int>> GetOutOfStockCountAsync(int? branchId = null, CancellationToken ct = default)
        {
            var count = await _unitOfWork.Stocks.GetOutOfStockCountAsync(branchId, ct);
            return _resultHandler.Success(count);
        }
        public async Task<ServiceResult<StockDto>> InitializeStockAsync(InitializeStockDto dto, CancellationToken ct = default)
        {
            var validation = await _initializeValidator.ValidateAsync(dto, ct);
            if (!validation.IsValid)
                throw new ValidationException(validation.Errors);

            if (dto.ProductVariantId.HasValue)
            {
                var product = await _unitOfWork.Products.GetByIdWithVariantsAsync(dto.ProductId, ct);
                var variantExists = product?.Variants.Any(v => v.Id == dto.ProductVariantId.Value) ?? false;

                if (!variantExists)
                    return _resultHandler.BadRequest<StockDto>("Variant does not belong to this product.");
            }

            var stock = new Stock
            {
                ProductId = dto.ProductId,
                ProductVariantId = dto.ProductVariantId,
                BranchId = dto.BranchId,
                Quantity = dto.Quantity,
                ReservedQuantity = 0
            };

            await _unitOfWork.Stocks.AddAsync(stock, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var savedStock = await _unitOfWork.Stocks.GetByIdWithDetailsAsync(stock.Id, ct);
            return _resultHandler.Created(_mapper.Map<StockDto>(savedStock));
        }

        public async Task<ServiceResult<StockDto>> AdjustStockAsync(AdjustStockDto dto, CancellationToken ct = default)
        {
            var validation = await _adjustValidator.ValidateAsync(dto, ct);
            if (!validation.IsValid)
                throw new ValidationException(validation.Errors);

            var stock = await _unitOfWork.Stocks.GetByProductAndBranchAsync(
                dto.ProductId, dto.ProductVariantId, dto.BranchId, ct);

            if (stock is null)
                return _resultHandler.NotFound<StockDto>("Stock record not found. Initialize it first.");

            switch (dto.AdjustmentType)
            {
                case StockAdjustmentType.Increase:
                    stock.Quantity += dto.Quantity;
                    break;

                case StockAdjustmentType.Decrease:
                    if (stock.Quantity - dto.Quantity < stock.ReservedQuantity)
                        return _resultHandler.BadRequest<StockDto>(
                            $"Cannot decrease below reserved quantity ({stock.ReservedQuantity}).");
                    stock.Quantity -= dto.Quantity;
                    break;

                case StockAdjustmentType.SetAbsolute:
                    if (dto.Quantity < stock.ReservedQuantity)
                        return _resultHandler.BadRequest<StockDto>(
                            $"Cannot set quantity below reserved quantity ({stock.ReservedQuantity}).");
                    stock.Quantity = dto.Quantity;
                    break;
            }

            _unitOfWork.Stocks.Update(stock);
            await _unitOfWork.SaveChangesAsync(ct);

            var updatedStock = await _unitOfWork.Stocks.GetByIdWithDetailsAsync(stock.Id, ct);
            return _resultHandler.Success(_mapper.Map<StockDto>(updatedStock));
        }

        public async Task<ServiceResult<bool>> TransferStockAsync(TransferStockDto dto, CancellationToken ct = default)
        {
            var validation = await _transferValidator.ValidateAsync(dto, ct);
            if (!validation.IsValid)
                throw new ValidationException(validation.Errors);

            await _unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable, ct);

            try
            {
                var sourceStock = await _unitOfWork.Stocks.GetByProductAndBranchAsync(
                    dto.ProductId, dto.ProductVariantId, dto.FromBranchId, ct);

                if (sourceStock is null || sourceStock.AvailableQuantity < dto.Quantity)
                {
                    await _unitOfWork.RollbackTransactionAsync(ct);
                    return _resultHandler.BadRequest<bool>("Not enough available stock in source branch.");
                }

                var destinationStock = await _unitOfWork.Stocks.GetByProductAndBranchAsync(
                    dto.ProductId, dto.ProductVariantId, dto.ToBranchId, ct);

                sourceStock.Quantity -= dto.Quantity;
                _unitOfWork.Stocks.Update(sourceStock);

                if (destinationStock is null)
                {
                    destinationStock = new Stock
                    {
                        ProductId = dto.ProductId,
                        ProductVariantId = dto.ProductVariantId,
                        BranchId = dto.ToBranchId,
                        Quantity = dto.Quantity,
                        ReservedQuantity = 0
                    };
                    await _unitOfWork.Stocks.AddAsync(destinationStock, ct);
                }
                else
                {
                    destinationStock.Quantity += dto.Quantity;
                    _unitOfWork.Stocks.Update(destinationStock);
                }

                await _unitOfWork.CommitTransactionAsync(ct);
                return _resultHandler.Success(true);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(ct);
                throw;
            }
        }

        public async Task<ServiceResult<List<StockDto>>> GetStockByProductAsync(int productId, CancellationToken ct = default)
        {
            var stocks = await _unitOfWork.Stocks.GetAllByProductAsync(productId, ct);
            return _resultHandler.Success(_mapper.Map<List<StockDto>>(stocks));
        }

        public async Task<ServiceResult<StockDto>> GetStockAsync(
            int productId, int? productVariantId, int branchId, CancellationToken ct = default)
        {
            var stock = await _unitOfWork.Stocks.GetByProductAndBranchAsync(productId, productVariantId, branchId, ct);
            if (stock is null)
                return _resultHandler.NotFound<StockDto>("Stock record not found.");

            var stockWithDetails = await _unitOfWork.Stocks.GetByIdWithDetailsAsync(stock.Id, ct);
            return _resultHandler.Success(_mapper.Map<StockDto>(stockWithDetails));
        }

        public async Task ReserveStockAsync(
            int productId, int? productVariantId, int branchId, int quantity, CancellationToken ct = default)
        {
            var stock = await _unitOfWork.Stocks.GetByProductAndBranchAsync(productId, productVariantId, branchId, ct);

            if (stock is null)
                throw new InvalidOperationException("Stock record not found.");

            if (stock.AvailableQuantity < quantity)
                throw new InvalidOperationException(
                    $"Insufficient stock. Available: {stock.AvailableQuantity}, Requested: {quantity}");

            stock.ReservedQuantity += quantity;
            _unitOfWork.Stocks.Update(stock);
        }

        public async Task ReleaseReservedStockAsync(
            int productId, int? productVariantId, int branchId, int quantity, CancellationToken ct = default)
        {
            var stock = await _unitOfWork.Stocks.GetByProductAndBranchAsync(productId, productVariantId, branchId, ct);
            if (stock is null) return;

            stock.ReservedQuantity = Math.Max(0, stock.ReservedQuantity - quantity);
            _unitOfWork.Stocks.Update(stock);
        }

        public async Task DecreaseStockAsync(int productId, int? productVariantId, int branchId, int quantity, CancellationToken ct = default)
        {
            var stock = await _unitOfWork.Stocks.GetByProductVariantAndBranchAsync(productId, productVariantId, branchId, ct);

            if (stock is null)
                throw new InvalidOperationException("Stock record not found.");

            if (stock.Quantity < quantity)
                throw new InvalidOperationException(
                    $"Insufficient physical stock. Quantity: {stock.Quantity}, Requested: {quantity}");

            stock.Quantity -= quantity;
            stock.ReservedQuantity = Math.Max(0, stock.ReservedQuantity - quantity);

            _unitOfWork.Stocks.Update(stock);
        }
    }
}