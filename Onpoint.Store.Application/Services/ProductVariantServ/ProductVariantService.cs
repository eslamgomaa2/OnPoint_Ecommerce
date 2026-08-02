using AutoMapper;
using BuildingBlocks.Results;
using FluentValidation;
using Onpoint.Store.Application.DTOs.Product;
using Onpoint.Store.Application.DTOs.ProductVariant;
using Onpoint.Store.Application.Services.CodeGeneration.BarcodeGeneration;
using Onpoint.Store.Application.Services.CodeGeneration.QrCodeGeneration;
using Onpoint.Store.Application.Services.CodeGeneration.SkuGeneration;
using Onpoint.Store.Application.Services.MedioServices;
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
        private readonly IMediaService _mediaService;

        public ProductVariantService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ServiceResultHandler resultHandler,
            IValidator<CreateProductVariantDto> addValidator,
            IValidator<UpdateProductVariantDto> updateValidator,
            ISkuGeneratorService skuService,
            IBarcodeService barcodeService,
            IQrCodeService qrCodeService,
            IMediaService mediaService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _resultHandler = resultHandler;
            _addValidator = addValidator;
            _updateValidator = updateValidator;
            _skuService = skuService;
            _barcodeService = barcodeService;
            _qrCodeService = qrCodeService;
            _mediaService = mediaService;
        }

        // ============================================================================
        // GET ALL VARIANTS (Admin)
        // ============================================================================
        public async Task<ServiceResult<List<ProductVariantDto>>> GetAllVariantsAsync(ProductVariantFilterRequestDto filter, CancellationToken ct = default)
        {
            var variants = await _unitOfWork.ProductVariants.GetAllVariants(
                filter.Sku,
                filter.MinPrice,
                filter.MaxPrice,
                filter.MinCost,
                filter.MaxCost,
                filter.IsActive,
                filter.ProductId,
                filter.SearchTerm,
                ct);

            var dtos = _mapper.Map<List<ProductVariantDto>>(variants);
            return _resultHandler.Success(dtos);
        }

        // ============================================================================
        // GET VARIANTS BY PRODUCT ID
        // ============================================================================
        public async Task<ServiceResult<List<ProductVariantDto>>> GetVariantsByProductIdAsync(int productId, CancellationToken ct = default)
        {
            var variants = await _unitOfWork.ProductVariants.GetByProductIdAsync(productId, ct);

            var dtos = variants.Select(v => MapToDto(v)).ToList();

            return _resultHandler.Success(dtos);
        }

        // ============================================================================
        // GET VARIANT BY ID
        // ============================================================================
        public async Task<ServiceResult<ProductVariantDto>> GetVariantByIdAsync(int id, CancellationToken ct = default)
        {
            var variant = await _unitOfWork.ProductVariants.GetByIdWithDetailsAsync(id, ct);

            if (variant == null)
                return _resultHandler.NotFound<ProductVariantDto>("Variant not found.");

            return _resultHandler.Success(MapToDto(variant));
        }

        // ============================================================================
        // ADD VARIANT TO PRODUCT
        // ============================================================================
        public async Task<ServiceResult<ProductVariantDto>> AddVariantAsync(int productId, CreateProductVariantDto dto, CancellationToken ct = default)
        {
            var validationResult = await _addValidator.ValidateAsync(dto, ct);
            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                return _resultHandler.BadRequest<ProductVariantDto>(errors);
            }

            var product = await _unitOfWork.Products.GetByIdWithVariantsAsync(productId, ct);
            if (product is null)
                return _resultHandler.NotFound<ProductVariantDto>("Product not found.");


            var skuToUse = dto.SkuMode == CodeGenerationMode.Manual
                ? dto.Sku!
                : await _skuService.GenerateUniqueSkuAsync($"{product.Name}-VAR", product.CategoryId);

            if (dto.SkuMode == CodeGenerationMode.Manual && await _unitOfWork.ProductVariants.SkuExistsAsync(skuToUse, null, ct))
                return _resultHandler.BadRequest<ProductVariantDto>($"SKU '{skuToUse}' already exists.");

            var variant = new ProductVariant
            {
                ProductId = productId,
                Price = dto.Price,
                Cost = dto.Cost,
                IsActive = true,
                Sku = skuToUse
            };


            // Barcode
            if (dto.BarcodeMode is not null)
            {
                variant.Barcode = dto.BarcodeMode == CodeGenerationMode.Manual
                    ? dto.Barcode
                    : await GenerateUniqueBarcodeAsync(ct);

                if (!string.IsNullOrEmpty(variant.Barcode))
                {
                    byte[] bytes = _barcodeService.GenerateImage(variant.Barcode);
                    using var stream = new MemoryStream(bytes);
                    var upload = await _mediaService.UploadProductImageAsync(new DTOs.Media.FileUploadDto
                    {
                        FileName = $"{variant.Sku}-barcode.png",
                        FileContent = stream
                    }, ct);
                    if (upload.Succeeded)
                        variant.BarcodeImagePath = upload.Data;
                }
            }

            // QR Code
            if (dto.QrCodeMode is not null)
            {
                variant.QrCodeValue = dto.QrCodeMode == CodeGenerationMode.Manual ? dto.QrCodeValue : _qrCodeService.GenerateValue(variant.Sku);

                if (!string.IsNullOrEmpty(variant.QrCodeValue))
                {
                    byte[] qrBytes = _qrCodeService.GenerateImage(variant.QrCodeValue);
                    using var qrStream = new MemoryStream(qrBytes);
                    var qrUpload = await _mediaService.UploadProductImageAsync(new DTOs.Media.FileUploadDto
                    {
                        FileName = $"{variant.Sku}-qr.png",
                        FileContent = qrStream
                    }, ct);
                    if (qrUpload.Succeeded)
                        variant.QrCodeImagePath = qrUpload.Data;
                }
            }

            // Attributes
            foreach (var a in dto.Attributes)
                variant.AttributeValues.Add(new VariantAttributeValue { ProductAttributeId = a.ProductAttributeId, Value = a.Value });

            // Stocks
            foreach (var bs in dto.BranchStocks)
            {
                variant.Stocks.Add(new Stock
                {
                    ProductId = productId,
                    BranchId = bs.BranchId,
                    Quantity = bs.Quantity,
                    ReservedQuantity = 0,
                    MinimumStockLevel = bs.MinimumStockLevel
                });
            }

            await _unitOfWork.ProductVariants.AddAsync(variant, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var saved = await _unitOfWork.ProductVariants.GetByIdWithDetailsAsync(variant.Id, ct);
            return _resultHandler.Created(MapToDto(saved!));
        }

        // ============================================================================
        // UPDATE VARIANT
        // ============================================================================
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
            variant.Cost = dto.Cost;

            // SKU
            if (dto.SkuMode == CodeGenerationMode.Manual && !string.IsNullOrWhiteSpace(dto.Sku) && dto.Sku != variant.Sku)
            {
                if (await _unitOfWork.ProductVariants.SkuExistsAsync(dto.Sku, variantId, ct))
                    return _resultHandler.BadRequest<ProductVariantDto>($"SKU '{dto.Sku}' already exists.");

                variant.Sku = dto.Sku;
            }

            // Barcode
            if (dto.BarcodeMode is not null)
            {
                var newBarcode = dto.BarcodeMode == CodeGenerationMode.Manual ? dto.Barcode : _barcodeService.GenerateValue();

                if (newBarcode != variant.Barcode)
                {
                    variant.Barcode = newBarcode;

                    if (!string.IsNullOrEmpty(variant.Barcode))
                    {
                        byte[] bytes = _barcodeService.GenerateImage(variant.Barcode);
                        using var stream = new MemoryStream(bytes);
                        var upload = await _mediaService.UploadProductImageAsync(new DTOs.Media.FileUploadDto
                        {
                            FileName = $"{variant.Sku}-barcode.png",
                            FileContent = stream
                        }, ct);
                        if (upload.Succeeded)
                            variant.BarcodeImagePath = upload.Data;
                    }
                }
            }

            // QR Code
            if (dto.QrCodeMode is not null)
            {
                var newQrValue = dto.QrCodeMode == CodeGenerationMode.Manual
                    ? dto.QrCodeValue
                    : _qrCodeService.GenerateValue(variant.Sku);

                if (newQrValue != variant.QrCodeValue)
                {
                    variant.QrCodeValue = newQrValue;
                    byte[] qrBytes = _qrCodeService.GenerateImage(variant.QrCodeValue);
                    using var qrStream = new MemoryStream(qrBytes);
                    var qrUpload = await _mediaService.UploadProductImageAsync(new DTOs.Media.FileUploadDto
                    {
                        FileName = $"{variant.Sku}-qr.png",
                        FileContent = qrStream
                    }, ct);
                    if (qrUpload.Succeeded)
                        variant.QrCodeImagePath = qrUpload.Data;
                }
            }

            // Attributes
            var existingAttrIds = variant.AttributeValues.Select(a => a.Id).ToList();
            _unitOfWork.VariantAttributes.RemoveRange(
                variant.AttributeValues.Where(a => true));
            variant.AttributeValues.Clear();

            foreach (var a in dto.Attributes)
                variant.AttributeValues.Add(new VariantAttributeValue { ProductAttributeId = a.ProductAttributeId, Value = a.Value });

            foreach (var bs in dto.BranchStocks)
            {
                var stock = variant.Stocks.FirstOrDefault(s => s.BranchId == bs.BranchId);
                if (stock != null)
                {
                    stock.Quantity = bs.Quantity;
                    stock.MinimumStockLevel = bs.MinimumStockLevel;
                }
                else
                {
                    variant.Stocks.Add(new Stock
                    {
                        ProductId = productId,
                        BranchId = bs.BranchId,
                        Quantity = bs.Quantity,
                        ReservedQuantity = 0,
                        MinimumStockLevel = bs.MinimumStockLevel
                    });
                }
            }

            variant.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.ProductVariants.Update(variant);
            await _unitOfWork.SaveChangesAsync(ct);

            return _resultHandler.Success(MapToDto(variant));
        }

        // ============================================================================
        // DELETE VARIANT (Soft Delete)
        // ============================================================================
        public async Task<ServiceResult<string>> DeleteVariantAsync(int productId, int variantId, CancellationToken ct = default)
        {
            var variant = await _unitOfWork.ProductVariants.GetByIdAsync(variantId, ct);
            if (variant is null || variant.ProductId != productId)
                return _resultHandler.NotFound<string>("Variant not found for this product.");

            var product = await _unitOfWork.Products.GetByIdWithVariantsAsync(productId, ct);
            var activeVariantsCount = product?.Variants.Count(v => v.IsActive && v.Id != variantId) ?? 0;

            if (activeVariantsCount == 0)
                return _resultHandler.BadRequest<string>("Cannot delete the last variant. Product must have at least one variant.");


            _unitOfWork.ProductVariants.Remove(variant);
            await _unitOfWork.SaveChangesAsync(ct);

            return _resultHandler.Success<string>("Variant deleted successfully.");
        }

        // ============================================================================
        // TOGGLE ACTIVE STATUS
        // ============================================================================
        public async Task<ServiceResult<string>> ToggleVariantStatusAsync(int productId, int variantId, CancellationToken ct = default)
        {
            var variant = await _unitOfWork.ProductVariants.GetByIdAsync(variantId, ct);
            if (variant is null || variant.ProductId != productId)
                return _resultHandler.NotFound<string>("Variant not found for this product.");

            // If deactivating, check if at least one variant remains active
            if (variant.IsActive)
            {
                var product = await _unitOfWork.Products.GetByIdWithVariantsAsync(productId, ct);
                var activeVariantsCount = product?.Variants.Count(v => v.IsActive && v.Id != variantId) ?? 0;

                if (activeVariantsCount == 0)
                    return _resultHandler.BadRequest<string>("Cannot deactivate the last variant. Product must have at least one active variant.");
            }

            variant.IsActive = !variant.IsActive;
            variant.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.ProductVariants.Update(variant);
            await _unitOfWork.SaveChangesAsync(ct);

            return _resultHandler.Success<string>($"Variant {(variant.IsActive ? "activated" : "deactivated")} successfully.");
        }

        // ============================================================================
        // HELPER: Map Entity to DTO
        // ============================================================================
        private ProductVariantDto MapToDto(ProductVariant v)
        {
            return new ProductVariantDto
            {
                Id = v.Id,
                ProductId = v.ProductId,
                Sku = v.Sku,
                Barcode = v.Barcode,
                BarcodeImagePath = v.BarcodeImagePath,
                QrCodeValue = v.QrCodeValue,
                QrCodeImagePath = v.QrCodeImagePath,
                Price = v.Price,
                Cost = v.Cost,
                IsActive = v.IsActive,
                Attributes = v.AttributeValues?.Select(av => new VariantAttributeValueDto
                {
                    ProductAttributeId = av.ProductAttributeId,
                    AttributeName = av.ProductAttribute?.Name ?? string.Empty,
                    Value = av.Value
                }).ToList() ?? new List<VariantAttributeValueDto>()

            };
        }
        private async Task<string> GenerateUniqueBarcodeAsync(CancellationToken ct, int maxAttempts = 5)
        {
            for (var i = 0; i < maxAttempts; i++)
            {
                var candidate = _barcodeService.GenerateValue();
                var existing = await _unitOfWork.ProductVariants.GetExistingBarcodesAsync(new List<string> { candidate }, ct);
                if (!existing.Any())
                    return candidate;
            }
            throw new InvalidOperationException("Failed to generate a unique barcode after multiple attempts.");
        }
    }
}