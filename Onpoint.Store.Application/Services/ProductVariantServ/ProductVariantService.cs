using AutoMapper;
using BuildingBlocks.Results;
using FluentValidation;
using Onpoint.Store.Application.DTOs.Product;
using Onpoint.Store.Application.DTOs.ProductVariant;
using Onpoint.Store.Application.Services.CodeGeneration.BarcodeGeneration;
using Onpoint.Store.Application.Services.CodeGeneration.QrCodeGeneration;
using Onpoint.Store.Application.Services.CodeGeneration.SkuGeneration;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Enums;
using Onpoint.Store.Domin.Repositories;

namespace Onpoint.Store.Application.Services.ProductVariantServ
{
    public class ProductVariantService : IProductVariantService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ServiceResultHandler _resultHandler;
        private readonly IValidator<CreateProductVariantDto> _addValidator;
        private readonly IValidator<UpdateProductVariantDto> _updateValidator;
        private readonly ISkuGeneratorService _skuService;
        private readonly IBarcodeService _barcodeService;
        private readonly IQrCodeService _qrCodeService;
        private readonly IImageStorageService _imageStorageService;

        public ProductVariantService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ServiceResultHandler resultHandler,
            IValidator<CreateProductVariantDto> addValidator,
            IValidator<UpdateProductVariantDto> updateValidator,
            ISkuGeneratorService skuService,
            IBarcodeService barcodeService,
            IQrCodeService qrCodeService,
            IImageStorageService imageStorageService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _resultHandler = resultHandler;
            _addValidator = addValidator;
            _updateValidator = updateValidator;
            _skuService = skuService;
            _barcodeService = barcodeService;
            _qrCodeService = qrCodeService;
            _imageStorageService = imageStorageService;
        }

        public async Task<ServiceResult<ProductVariantDto>> AddVariantAsync(int productId, CreateProductVariantDto dto, CancellationToken ct = default)
        {
            var validationResult = await _addValidator.ValidateAsync(dto, ct);
            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                return _resultHandler.BadRequest<ProductVariantDto>(errors);
            }

            var product = await _unitOfWork.Products.GetWithFullDetailsForAdminAsync(productId, ct);
            if (product is null)
                return _resultHandler.NotFound<ProductVariantDto>("Product not found.");

            var variant = new ProductVariant
            {
                ProductId = productId,
                Price = dto.Price,
                IsActive = true,
                Sku = dto.SkuMode == CodeGenerationMode.Manual
                    ? dto.Sku!
                    : await _skuService.GenerateUniqueSkuAsync($"{product.Name}-VAR", product.CategoryId)
            };

            if (dto.BarcodeMode is not null)
            {
                variant.Barcode = dto.BarcodeMode == CodeGenerationMode.Manual ? dto.Barcode : _barcodeService.GenerateValue();

                var bytes = _barcodeService.GenerateImage(variant.Barcode!);
                using var stream = new MemoryStream(bytes);

                var upload = await _imageStorageService.UploadImageAsync(stream, $"{variant.Sku}-barcode.png", ct);
                if (!upload.Succeeded)
                    return _resultHandler.BadRequest<ProductVariantDto>(upload.Message ?? "Failed to upload variant barcode image.");

                variant.BarcodeImagePath = upload.Data!;
            }

            if (dto.QrCodeMode is not null)
            {
                var qrValue = _qrCodeService.GenerateValue(variant.Sku);
                variant.QrCodeValue = qrValue;

                var qrBytes = _qrCodeService.GenerateImage(qrValue);
                using var qrStream = new MemoryStream(qrBytes);

                var qrUpload = await _imageStorageService.UploadImageAsync(qrStream, $"{variant.Sku}-qr.png", ct);
                if (!qrUpload.Succeeded)
                    return _resultHandler.BadRequest<ProductVariantDto>(qrUpload.Message ?? "Failed to upload variant QR image.");

                variant.QrCodeImagePath = qrUpload.Data!;
            }

            foreach (var a in dto.Attributes)
                variant.AttributeValues.Add(new VariantAttributeValue { ProductAttributeId = a.ProductAttributeId, Value = a.Value });

            foreach (var bs in dto.BranchStocks)
                variant.Stocks.Add(new Stock { BranchId = bs.BranchId, Quantity = bs.Quantity, ReservedQuantity = 0 });

            await _unitOfWork.ProductVariants.AddAsync(variant, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var saved = await _unitOfWork.ProductVariants.GetByIdWithDetailsAsync(variant.Id, ct);
            return _resultHandler.Created(_mapper.Map<ProductVariantDto>(saved));
        }

        public async Task<ServiceResult<ProductVariantDto>> UpdateVariantAsync(int productId, int variantId, UpdateProductVariantDto dto, CancellationToken ct = default)
        {
            var validationResult = await _updateValidator.ValidateAsync(dto, ct);
            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                return _resultHandler.BadRequest<ProductVariantDto>(errors);
            }

            var variant = await _unitOfWork.ProductVariants.GetByIdWithDetailsAsync(variantId, ct);
            if (variant is null || variant.ProductId != productId)
                return _resultHandler.NotFound<ProductVariantDto>("Variant not found for this product.");

            variant.Price = dto.Price;

            if (dto.SkuMode == CodeGenerationMode.Manual && !string.IsNullOrWhiteSpace(dto.Sku) && dto.Sku != variant.Sku)
            {
                if (await _unitOfWork.Products.SkuExistsAsync(dto.Sku, null, ct))
                    return _resultHandler.BadRequest<ProductVariantDto>($"SKU '{dto.Sku}' already exists.");

                variant.Sku = dto.Sku;
            }

            if (dto.BarcodeMode is not null)
            {
                var newBarcode = dto.BarcodeMode == CodeGenerationMode.Manual ? dto.Barcode : _barcodeService.GenerateValue();

                if (newBarcode != variant.Barcode)
                {
                    variant.Barcode = newBarcode;

                    var bytes = _barcodeService.GenerateImage(variant.Barcode!);
                    using var stream = new MemoryStream(bytes);

                    var upload = await _imageStorageService.UploadImageAsync(stream, $"{variant.Sku}-barcode.png", ct);
                    if (!upload.Succeeded)
                        return _resultHandler.BadRequest<ProductVariantDto>(upload.Message ?? "Failed to upload variant barcode image.");

                    variant.BarcodeImagePath = upload.Data!;
                }
            }

            if (dto.QrCodeMode is not null && string.IsNullOrEmpty(variant.QrCodeValue))
            {
                var qrValue = _qrCodeService.GenerateValue(variant.Sku);
                variant.QrCodeValue = qrValue;

                var qrBytes = _qrCodeService.GenerateImage(qrValue);
                using var qrStream = new MemoryStream(qrBytes);

                var qrUpload = await _imageStorageService.UploadImageAsync(qrStream, $"{variant.Sku}-qr.png", ct);
                if (!qrUpload.Succeeded)
                    return _resultHandler.BadRequest<ProductVariantDto>(qrUpload.Message ?? "Failed to upload variant QR image.");

                variant.QrCodeImagePath = qrUpload.Data!;
            }

            variant.AttributeValues.Clear();
            foreach (var a in dto.Attributes)
                variant.AttributeValues.Add(new VariantAttributeValue { ProductAttributeId = a.ProductAttributeId, Value = a.Value });

            foreach (var bs in dto.BranchStocks)
            {
                var stock = variant.Stocks.FirstOrDefault(s => s.BranchId == bs.BranchId);
                if (stock != null)
                    stock.Quantity = bs.Quantity;
                else
                    variant.Stocks.Add(new Stock { BranchId = bs.BranchId, Quantity = bs.Quantity, ReservedQuantity = 0 });
            }

            variant.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.ProductVariants.Update(variant);
            await _unitOfWork.SaveChangesAsync(ct);

            return _resultHandler.Success(_mapper.Map<ProductVariantDto>(variant));
        }

        public async Task<ServiceResult<string>> DeactivateVariantAsync(int productId, int variantId, CancellationToken ct = default)
        {
            var variant = await _unitOfWork.ProductVariants.GetByIdAsync(variantId, ct);
            if (variant is null || variant.ProductId != productId)
                return _resultHandler.NotFound<string>("Variant not found for this product.");

            variant.IsActive = false;
            variant.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.ProductVariants.Update(variant);
            await _unitOfWork.SaveChangesAsync(ct);

            return _resultHandler.Success<string>("Variant deactivated successfully.");
        }

        public async Task<ServiceResult<string>> ActivateVariantAsync(int productId, int variantId, CancellationToken ct = default)
        {
            var variant = await _unitOfWork.ProductVariants.GetByIdAsync(variantId, ct);
            if (variant is null || variant.ProductId != productId)
                return _resultHandler.NotFound<string>("Variant not found for this product.");

            variant.IsActive = true;
            variant.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.ProductVariants.Update(variant);
            await _unitOfWork.SaveChangesAsync(ct);

            return _resultHandler.Success<string>("Variant activated successfully.");
        }
    }
}