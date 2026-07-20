using AutoMapper;
using BuildingBlocks.Results;
using FluentValidation;
using Onpoint.Store.Application.DTOs;
using Onpoint.Store.Application.DTOs.Media;
using Onpoint.Store.Application.DTOs.Product;
using Onpoint.Store.Application.Services.CodeGeneration.BarcodeGeneration;
using Onpoint.Store.Application.Services.CodeGeneration.QrCodeGeneration;
using Onpoint.Store.Application.Services.CodeGeneration.SkuGeneration;
using Onpoint.Store.Application.Services.MedioServices;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Enums;
using Onpoint.Store.Domin.Repositories;

namespace Onpoint.Store.Application.Services.ProductServ
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ServiceResultHandler _resultHandler;
        private readonly IMediaService _mediaService;
        private readonly IValidator<CreateProductDto> _createValidator;
        private readonly IValidator<UpdateProductDto> _updateValidator;
        private readonly ISkuGeneratorService _skuGeneratorService;
        private readonly IBarcodeService _barcodeService;
        private readonly IQrCodeService _qrCodeService;
        private readonly IImageStorageService _imageStorageService;

        public ProductService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ServiceResultHandler resultHandler,
            IValidator<CreateProductDto> createValidator,
            IValidator<UpdateProductDto> updateValidator,
            ISkuGeneratorService skuGenerator,
            IBarcodeService barcodeService,
            IQrCodeService qrCodeService,
            IImageStorageService imageStorageService,
            IMediaService mediaService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _resultHandler = resultHandler;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _skuGeneratorService = skuGenerator;
            _barcodeService = barcodeService;
            _qrCodeService = qrCodeService;
            _imageStorageService = imageStorageService;
            _mediaService = mediaService;
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


            product.Sku = dto.SkuMode == CodeGenerationMode.Manual
                ? dto.Sku!
                : await _skuGeneratorService.GenerateUniqueSkuAsync(dto.Name, dto.CategoryId);

            product.Status = dto.Status;


            product.Barcode = dto.BarcodeMode == CodeGenerationMode.Manual
                ? dto.Barcode!
                : _barcodeService.GenerateValue();

            if (!string.IsNullOrEmpty(product.Barcode))
            {
                byte[] barcodeBytes = _barcodeService.GenerateImage(product.Barcode);
                using var barcodeStream = new MemoryStream(barcodeBytes);

                var barcodeUploadResult = await _mediaService.UploadProductImageAsync(new FileUploadDto
                {
                    FileName = $"barcode_{product.Barcode}.png",
                    FileContent = barcodeStream
                }, ct);

                if (barcodeUploadResult.Succeeded)
                    product.BarcodeImagePath = barcodeUploadResult.Data;
            }

            // 4. QR Code Generation
            product.QrCodeValue = dto.QrCodeMode == CodeGenerationMode.Manual
                ? dto.QrCodeValue!
                : _qrCodeService.GenerateValue(product.Sku);

            if (!string.IsNullOrEmpty(product.QrCodeValue))
            {
                byte[] qrCodeBytes = _qrCodeService.GenerateImage(product.QrCodeValue);
                using var qrStream = new MemoryStream(qrCodeBytes);

                var qrUploadResult = await _mediaService.UploadProductImageAsync(new FileUploadDto
                {
                    FileName = $"qrcode_{product.Sku}.png",
                    FileContent = qrStream
                }, ct);

                if (qrUploadResult.Succeeded)
                    product.QrCodeImagePath = qrUploadResult.Data;
            }


            if (dto.Images?.Any() == true)
            {
                foreach (var img in dto.Images)
                    product.Images.Add(new ProductImage { ImageUrl = img.ImageUrl, IsPrimary = img.IsPrimary });
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


            if (dto.Attributes?.Any() == true)
            {
                foreach (var attr in dto.Attributes)
                {
                    product.AttributeValues.Add(new ProductAttributeValue
                    {
                        ProductAttributeId = attr.ProductAttributeId,
                        Value = attr.Value
                    });
                }
            }

            if (dto.Variants?.Any() == true)
            {
                foreach (var v in dto.Variants)
                {
                    var variant = new ProductVariant
                    {
                        Price = v.Price,
                        IsActive = true,
                        Sku = v.SkuMode == CodeGenerationMode.Manual
                            ? v.Sku!
                            : await _skuGeneratorService.GenerateUniqueSkuAsync($"{dto.Name}-VAR", dto.CategoryId)
                    };

                    variant.Barcode = v.BarcodeMode == CodeGenerationMode.Manual
                        ? v.Barcode
                        : _barcodeService.GenerateValue();

                    if (!string.IsNullOrEmpty(variant.Barcode))
                    {
                        byte[] barcodeBytes = _barcodeService.GenerateImage(variant.Barcode);
                        using var barcodeStream = new MemoryStream(barcodeBytes);

                        var barcodeUploadResult = await _mediaService.UploadProductImageAsync(new FileUploadDto
                        {
                            FileName = $"barcode_var_{variant.Sku}.png",
                            FileContent = barcodeStream
                        }, ct);

                        if (barcodeUploadResult.Succeeded)
                            variant.BarcodeImagePath = barcodeUploadResult.Data;
                    }

                    variant.QrCodeValue = v.QrCodeMode == CodeGenerationMode.Manual
                        ? v.QrCodeValue
                        : _qrCodeService.GenerateValue(variant.Sku);

                    if (!string.IsNullOrEmpty(variant.QrCodeValue))
                    {
                        byte[] qrCodeBytes = _qrCodeService.GenerateImage(variant.QrCodeValue);
                        using var qrStream = new MemoryStream(qrCodeBytes);

                        var qrUploadResult = await _mediaService.UploadProductImageAsync(new FileUploadDto
                        {
                            FileName = $"qrcode_var_{variant.Sku}.png",
                            FileContent = qrStream
                        }, ct);

                        if (qrUploadResult.Succeeded)
                            variant.QrCodeImagePath = qrUploadResult.Data;
                    }

                    foreach (var a in v.Attributes)
                    {
                        variant.AttributeValues.Add(new VariantAttributeValue
                        {
                            ProductAttributeId = a.ProductAttributeId,
                            Value = a.Value
                        });
                    }

                    product.Variants.Add(variant);
                }
            }

            await _unitOfWork.Products.AddAsync(product, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            if (dto.Variants?.Any() == true)
            {
                foreach (var vDto in dto.Variants)
                {
                    var savedVariant = product.Variants.First(v => v.Sku == (vDto.SkuMode == CodeGenerationMode.Manual ? vDto.Sku! : v.Sku));

                    foreach (var bs in vDto.BranchStocks)
                    {
                        product.Stocks.Add(new Stock
                        {
                            ProductId = product.Id,          // ProductId حقيقي
                            ProductVariantId = savedVariant.Id, // ProductVariantId حقيقي
                            BranchId = bs.BranchId,
                            Quantity = bs.Quantity,
                            ReservedQuantity = 0
                        });
                    }
                }
            }
            else if (dto.BranchStocks?.Any() == true)
            {
                foreach (var bs in dto.BranchStocks)
                {
                    product.Stocks.Add(new Stock
                    {
                        ProductId = product.Id,
                        ProductVariantId = null,
                        BranchId = bs.BranchId,
                        Quantity = bs.Quantity,
                        ReservedQuantity = 0
                    });
                }
            }

            // حفظ المخزون
            await _unitOfWork.SaveChangesAsync(ct);

            return _resultHandler.Created(_mapper.Map<ProductDto>(product));
        }
        public async Task<ServiceResult<ProductDto>> UpdateAsync(int id, UpdateProductDto dto, CancellationToken ct = default)
        {
            var validationResult = await _updateValidator.ValidateAsync(dto, ct);
            if (!validationResult.IsValid)
                throw new FluentValidation.ValidationException(validationResult.Errors);

            var existingProduct = await _unitOfWork.Products.GetWithFullDetailsForAdminAsync(id, ct);
            if (existingProduct is null)
                return _resultHandler.NotFound<ProductDto>("Product not found to update");

            // 1. Check SKU uniqueness
            if (!string.IsNullOrWhiteSpace(dto.Sku) && dto.Sku != existingProduct.Sku)
            {
                if (await _unitOfWork.Products.SkuExistsAsync(dto.Sku, id, ct))
                    return _resultHandler.BadRequest<ProductDto>($"SKU '{dto.Sku}' already exists");

                existingProduct.Sku = dto.Sku;
            }

            // 2. Update basic fields
            existingProduct.Name = dto.Name;
            existingProduct.Slug = dto.Slug;
            existingProduct.Description = dto.Description;
            existingProduct.Price = dto.Price;
            existingProduct.CategoryId = dto.CategoryId;
            existingProduct.IsPopular = dto.IsPopular;
            existingProduct.Status = dto.Status;
            existingProduct.UpdatedAt = DateTime.UtcNow;


            if (dto.BarcodeMode is not null)
            {
                var newBarcode = dto.BarcodeMode == CodeGenerationMode.Manual
                    ? dto.Barcode
                    : _barcodeService.GenerateValue();

                if (newBarcode != existingProduct.Barcode)
                {
                    existingProduct.Barcode = newBarcode;

                    var barcodeBytes = _barcodeService.GenerateImage(existingProduct.Barcode!);
                    using var barcodeStream = new MemoryStream(barcodeBytes);

                    var barcodeUpload = await _imageStorageService.UploadImageAsync(
                        barcodeStream, $"{existingProduct.Sku}-barcode.png", ct);

                    if (!barcodeUpload.Succeeded)
                        throw new InvalidOperationException(barcodeUpload.Message ?? "Failed to upload barcode image.");

                    existingProduct.BarcodeImagePath = barcodeUpload.Data!;
                }
            }


            if (dto.QrCodeMode is not null)
            {
                var newQrValue = dto.QrCodeMode == CodeGenerationMode.Manual
                    ? dto.QrCodeValue
                    : _qrCodeService.GenerateValue(existingProduct.Sku);

                if (newQrValue != existingProduct.QrCodeValue)
                {
                    existingProduct.QrCodeValue = newQrValue;

                    var qrBytes = _qrCodeService.GenerateImage(existingProduct.QrCodeValue!);
                    using var qrStream = new MemoryStream(qrBytes);

                    var qrUpload = await _imageStorageService.UploadImageAsync(
                        qrStream, $"{existingProduct.Sku}-qr.png", ct);

                    if (!qrUpload.Succeeded)
                        throw new InvalidOperationException(qrUpload.Message ?? "Failed to upload QR image.");

                    existingProduct.QrCodeImagePath = qrUpload.Data!;
                }
            }

            // 5. Update Images
            existingProduct.Images.Clear();
            if (dto.Images?.Any() == true)
            {
                foreach (var img in dto.Images)
                    existingProduct.Images.Add(new ProductImage { ImageUrl = img.ImageUrl, IsPrimary = img.IsPrimary });
            }

            // 6. Update Discount
            existingProduct.Discounts.Clear();
            if (dto.Discount != null)
            {
                existingProduct.Discounts.Add(new Discount
                {
                    DiscountPercentage = dto.Discount.DiscountPercentage,
                    StartDate = dto.Discount.StartDate,
                    EndDate = dto.Discount.EndDate,
                    IsActive = true
                });
            }


            existingProduct.AttributeValues.Clear();
            foreach (var attr in dto.Attributes ?? Enumerable.Empty<CreateProductAttributeValueDto>())
            {
                existingProduct.AttributeValues.Add(new ProductAttributeValue
                {
                    ProductAttributeId = attr.ProductAttributeId,
                    Value = attr.Value
                });
            }

            var incomingVariantIds = dto.Variants.Where(v => v.Id.HasValue).Select(v => v.Id!.Value).ToList();

            foreach (var existingVariant in existingProduct.Variants.ToList())
            {
                if (!incomingVariantIds.Contains(existingVariant.Id))
                    existingVariant.IsActive = false;
            }

            foreach (var v in dto.Variants)
            {
                if (v.Id.HasValue)
                {
                    var variant = existingProduct.Variants.FirstOrDefault(x => x.Id == v.Id.Value);
                    if (variant is null)
                        return _resultHandler.BadRequest<ProductDto>($"Variant {v.Id} does not belong to this product.");

                    variant.Price = v.Price;
                    variant.IsActive = true;

                    if (v.SkuMode == CodeGenerationMode.Manual && !string.IsNullOrWhiteSpace(v.Sku) && v.Sku != variant.Sku)
                        variant.Sku = v.Sku;


                    if (v.BarcodeMode is not null)
                    {
                        var newBarcode = v.BarcodeMode == CodeGenerationMode.Manual ? v.Barcode : _barcodeService.GenerateValue();
                        if (newBarcode != variant.Barcode)
                        {
                            variant.Barcode = newBarcode;
                            var bytes = _barcodeService.GenerateImage(variant.Barcode!);
                            using var stream = new MemoryStream(bytes);
                            var upload = await _imageStorageService.UploadImageAsync(stream, $"{variant.Sku}-barcode.png", ct);
                            if (!upload.Succeeded)
                                throw new InvalidOperationException(upload.Message ?? "Failed to upload variant barcode image.");
                            variant.BarcodeImagePath = upload.Data!;
                        }
                    }


                    if (v.QrCodeMode is not null)
                    {
                        var newQrValue = v.QrCodeMode == CodeGenerationMode.Manual ? v.QrCodeValue : _qrCodeService.GenerateValue(variant.Sku);
                        if (newQrValue != variant.QrCodeValue)
                        {
                            variant.QrCodeValue = newQrValue;
                            var bytes = _qrCodeService.GenerateImage(variant.QrCodeValue!);
                            using var stream = new MemoryStream(bytes);
                            var upload = await _imageStorageService.UploadImageAsync(stream, $"{variant.Sku}-qr.png", ct);
                            if (!upload.Succeeded)
                                throw new InvalidOperationException(upload.Message ?? "Failed to upload variant QR code image.");
                            variant.QrCodeImagePath = upload.Data!;
                        }
                    }

                    variant.AttributeValues.Clear();
                    foreach (var a in v.Attributes)
                        variant.AttributeValues.Add(new VariantAttributeValue { ProductAttributeId = a.ProductAttributeId, Value = a.Value });

                    foreach (var bs in v.BranchStocks)
                    {
                        var stock = variant.Stocks.FirstOrDefault(s => s.BranchId == bs.BranchId);
                        if (stock != null)
                            stock.Quantity = bs.Quantity;
                        else
                        {
                            var newStock = new Stock { BranchId = bs.BranchId, Quantity = bs.Quantity, ReservedQuantity = 0 };
                            variant.Stocks.Add(newStock);
                        }
                    }
                }
                else
                {
                    var variant = new ProductVariant
                    {
                        Price = v.Price,
                        IsActive = true,
                        Sku = v.SkuMode == CodeGenerationMode.Manual
                            ? v.Sku!
                            : await _skuGeneratorService.GenerateUniqueSkuAsync($"{dto.Name}-VAR", dto.CategoryId)
                    };


                    if (v.BarcodeMode is not null)
                    {
                        variant.Barcode = v.BarcodeMode == CodeGenerationMode.Manual ? v.Barcode : _barcodeService.GenerateValue();
                        if (!string.IsNullOrEmpty(variant.Barcode))
                        {
                            var bytes = _barcodeService.GenerateImage(variant.Barcode);
                            using var stream = new MemoryStream(bytes);
                            var upload = await _imageStorageService.UploadImageAsync(stream, $"{variant.Sku}-barcode.png", ct);
                            if (!upload.Succeeded)
                                throw new InvalidOperationException(upload.Message ?? "Failed to upload variant barcode image.");
                            variant.BarcodeImagePath = upload.Data!;
                        }
                    }


                    variant.QrCodeValue = v.QrCodeMode == CodeGenerationMode.Manual
                        ? v.QrCodeValue
                        : _qrCodeService.GenerateValue(variant.Sku);

                    if (!string.IsNullOrEmpty(variant.QrCodeValue))
                    {
                        var bytes = _qrCodeService.GenerateImage(variant.QrCodeValue);
                        using var stream = new MemoryStream(bytes);
                        var upload = await _imageStorageService.UploadImageAsync(stream, $"{variant.Sku}-qr.png", ct);
                        if (!upload.Succeeded)
                            throw new InvalidOperationException(upload.Message ?? "Failed to upload variant QR code image.");
                        variant.QrCodeImagePath = upload.Data!;
                    }

                    foreach (var a in v.Attributes)
                        variant.AttributeValues.Add(new VariantAttributeValue { ProductAttributeId = a.ProductAttributeId, Value = a.Value });

                    foreach (var bs in v.BranchStocks)
                    {
                        var stock = new Stock { BranchId = bs.BranchId, Quantity = bs.Quantity, ReservedQuantity = 0 };
                        variant.Stocks.Add(stock);
                    }

                    existingProduct.Variants.Add(variant);
                }
            }


            if (!existingProduct.Variants.Any(v => v.IsActive))
            {
                foreach (var bs in dto.BranchStocks ?? Enumerable.Empty<VariantBranchStockDto>())
                {
                    var stock = existingProduct.Stocks.FirstOrDefault(s => s.BranchId == bs.BranchId && s.ProductVariantId == null);
                    if (stock != null)
                        stock.Quantity = bs.Quantity;
                    else
                        existingProduct.Stocks.Add(new Stock { BranchId = bs.BranchId, Quantity = bs.Quantity, ReservedQuantity = 0 });
                }
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
        public async Task<ServiceResult<ProductDto>> GetBySkuAsync(string sku, CancellationToken ct = default)
        {
            var product = await _unitOfWork.Products.GetBySkuAsync(sku, ct);
            if (product == null)
                return _resultHandler.NotFound<ProductDto>($"Product with SKU '{sku}' not found");

            return _resultHandler.Success(_mapper.Map<ProductDto>(product));
        }


        private async Task<string> GenerateSkuAsync(Product product, CancellationToken ct)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(product.CategoryId, ct);
            var categoryPrefix = category?.Name?.Length >= 3
                ? category.Name.Substring(0, 3).ToUpper()
                : "GEN";

            var namePrefix = product.Name?.Length >= 3
                ? product.Name.Substring(0, 3).ToUpper()
                : "PRD";

            var random = new Random();
            var sequence = random.Next(1000, 9999);

            var sku = $"{categoryPrefix}-{namePrefix}-{sequence}";


            if (await _unitOfWork.Products.SkuExistsAsync(sku))
            {
                sequence = random.Next(1000, 9999);
                sku = $"{categoryPrefix}-{namePrefix}-{sequence}";
            }

            return sku;
        }


    }
}
