using AutoMapper;
using BuildingBlocks.Results;
using FluentValidation;
using Onpoint.Store.Application.DTOs.Media;
using Onpoint.Store.Application.DTOs.Product;
using Onpoint.Store.Application.DTOs.Product.BranchManger;
using Onpoint.Store.Application.DTOs.ProductVariant;
using Onpoint.Store.Application.Helpers;
using Onpoint.Store.Application.Services.BranchManagerProductService;
using Onpoint.Store.Application.Services.CodeGeneration.BarcodeGeneration;
using Onpoint.Store.Application.Services.CodeGeneration.QrCodeGeneration;
using Onpoint.Store.Application.Services.CodeGeneration.SkuGeneration;
using Onpoint.Store.Application.Services.MedioServices;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Enums;
using Onpoint.Store.Domin.Repositories;

namespace Onpoint.Store.Application.Services.ProductServ
{
    public class BranchManagerProductService : IBranchManagerProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ServiceResultHandler _resultHandler;
        private readonly IMediaService _mediaService;
        private readonly IValidator<CreateProductByBranchManagerDto> _createValidator;
        private readonly IValidator<UpdateProductDto> _updateValidator;
        private readonly ISkuGeneratorService _skuGeneratorService;
        private readonly IBarcodeService _barcodeService;
        private readonly IQrCodeService _qrCodeService;
        private readonly IImageStorageService _imageStorageService;

        public BranchManagerProductService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ServiceResultHandler resultHandler,
            IValidator<CreateProductByBranchManagerDto> createValidator,
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

        public async Task<ServiceResult<ProductDashboardDto>> GetDashboardCountsAsync(int branchId, CancellationToken ct = default)
        {
            var inStock = await _unitOfWork.Stocks.GetInStockCountAsync(branchId, ct);
            var lowStock = await _unitOfWork.Stocks.GetLowStockCountAsync(branchId, ct);
            var outOfStock = await _unitOfWork.Stocks.GetOutOfStockCountAsync(branchId, ct);

            var dto = new ProductDashboardDto
            {
                TotalProducts = inStock + lowStock + outOfStock,
                InStock = inStock,
                LowStock = lowStock,
                OutOfStock = outOfStock
            };

            return _resultHandler.Success(dto);
        }

        public async Task<ServiceResult<PagedResult<ProductDto>>> GetFilteredPagedAsync(
            int branchId, PaginationRequest request, int? categoryId = null, string? searchTerm = null, LanguageCode? languageCode = null, CancellationToken ct = default)
        {
            var (items, totalCount) = await _unitOfWork.Products.GetFilteredPagedAsync(
                categoryId, searchTerm, branchId, request.PageNumber, request.PageSize, ct);

            var dtoItems = items.Select(p => ApplyTranslation(MapWithBranchStock(p, branchId), p, languageCode)).ToList();

            var pagedResult = PagedResult<ProductDto>.Create(dtoItems, totalCount, request.PageNumber, request.PageSize);
            return _resultHandler.Success(pagedResult);
        }

        public async Task<ServiceResult<ProductDetailDto>> GetByIdAsync(int branchId, int id, LanguageCode? languageCode = null, CancellationToken ct = default)
        {
            var product = await _unitOfWork.Products.GetWithDetailsAsync(id, ct);
            if (product is null)
                return _resultHandler.NotFound<ProductDetailDto>("Product not found");

            bool hasStockInBranch = product.Stocks.Any(s => s.BranchId == branchId && s.ProductVariantId == null) ||
                                     product.Variants.Any(v => v.IsActive && v.Stocks.Any(s => s.BranchId == branchId));

            if (!hasStockInBranch)
                return _resultHandler.NotFound<ProductDetailDto>("Product not available in this branch");

            var dto = _mapper.Map<ProductDetailDto>(product);

            var baseDto = MapWithBranchStock(product, branchId);
            dto.TotalStock = baseDto.TotalStock;
            dto.StockStatus = baseDto.StockStatus;

            if (!product.Variants.Any(v => v.IsActive))
            {
                var stocks = product.Stocks.Where(s => s.BranchId == branchId && s.ProductVariantId == null);
                dto.BranchStock = stocks.Select(s => _mapper.Map<VariantStockDto>(s)).ToList();
            }
            else
            {
                foreach (var variantDto in dto.Variants)
                {
                    variantDto.Stocks = variantDto.Stocks.Where(s => s.BranchId == branchId).ToList();
                }
            }

            ApplyTranslationToDetail(dto, product, languageCode);

            return _resultHandler.Success(dto);
        }

        public async Task<ServiceResult<ProductDto>> CreateAsync(int branchId, CreateProductByBranchManagerDto dto, CancellationToken ct = default)
        {
            var validationResult = await _createValidator.ValidateAsync(dto, ct);
            if (!validationResult.IsValid)
                throw new FluentValidation.ValidationException(validationResult.Errors);

            var branchExists = await _unitOfWork.Branches.GetByIdAsync(branchId, ct);
            if (branchExists is null)
            {
                return _resultHandler.NotFound<ProductDto>("Branch not found");
            }

            var product = _mapper.Map<Product>(dto);

            product.Stocks.Clear();

            if (product.Translations?.Any() == true)
            {
                product.Translations = product.Translations
                    .GroupBy(t => t.LanguageCode)
                    .Select(g => g.First())
                    .ToList();
            }

            product.Sku = dto.SkuMode == CodeGenerationMode.Manual
                ? dto.Sku!
                : await _skuGeneratorService.GenerateUniqueSkuAsync(dto.Name, dto.CategoryId);

            product.Status = dto.Status;
            product.Slug = SlugHelper.GenerateSlug(product.Name);
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

            // 4. الخصم (Discount)
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

            var variantTracker = new List<(CreateProductVariantByBranchManagerDto Dto, ProductVariant Entity)>();

            // 5. إنشاء الـ Variants
            if (dto.Variants?.Any() == true)
            {
                product.Variants.Clear();

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

                    if (v.Attributes?.Any() == true)
                    {
                        foreach (var a in v.Attributes)
                        {
                            variant.AttributeValues.Add(new VariantAttributeValue
                            {
                                ProductAttributeId = a.ProductAttributeId,
                                Value = a.Value
                            });
                        }
                    }

                    product.Variants.Add(variant);
                    variantTracker.Add((v, variant));
                }
            }

            await _unitOfWork.Products.AddAsync(product, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            // 7. إضافة المخزون (Stock)
            if (variantTracker.Any())
            {
                foreach (var (vDto, savedVariant) in variantTracker)
                {
                    if (vDto.BranchStocks?.Any() == true)
                    {
                        foreach (var bs in vDto.BranchStocks)
                        {
                            var stock = new Stock
                            {
                                ProductId = product.Id,
                                ProductVariantId = savedVariant.Id,
                                BranchId = branchId,
                                Quantity = bs.Quantity,
                                ReservedQuantity = 0,
                                MinimumStockLevel = dto.MinimumStockLevel != 0 ? dto.MinimumStockLevel : 0
                            };

                            product.Stocks.Add(stock);
                            savedVariant.Stocks.Add(stock);
                        }
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
                        BranchId = branchId,
                        Quantity = bs.Quantity,
                        ReservedQuantity = 0,
                        MinimumStockLevel = dto.MinimumStockLevel != 0 ? dto.MinimumStockLevel : 0
                    });
                }
            }

            // 8. حفظ تغيرات المخزون النهائية
            await _unitOfWork.SaveChangesAsync(ct);

            return _resultHandler.Created(_mapper.Map<ProductDto>(product));
        }
        public async Task<ServiceResult<ProductDto>> UpdateAsync(int branchId, int id, UpdateProductDto dto, CancellationToken ct = default)
        {
            var validationResult = await _updateValidator.ValidateAsync(dto, ct);
            if (!validationResult.IsValid)
                throw new FluentValidation.ValidationException(validationResult.Errors);

            var existingProduct = await _unitOfWork.Products.GetWithStocksForBranchCheckAsync(id, ct);
            if (existingProduct is null)
                return _resultHandler.NotFound<ProductDto>("Product not found to update");

            bool hasStockInBranch = existingProduct.Stocks.Any(s => s.BranchId == branchId && s.ProductVariantId == null) ||
                                     existingProduct.Variants.Any(v => v.IsActive && v.Stocks.Any(s => s.BranchId == branchId));

            if (!hasStockInBranch)
                return _resultHandler.BadRequest<ProductDto>("Product not available in this branch");

            // Sku unique check لو اتغير
            if (!string.IsNullOrWhiteSpace(dto.Sku) && dto.Sku != existingProduct.Sku)
            {
                if (await _unitOfWork.Products.SkuExistsAsync(dto.Sku, id, ct))
                    return _resultHandler.BadRequest<ProductDto>($"SKU '{dto.Sku}' already exists");

                existingProduct.Sku = dto.Sku;
            }


            existingProduct.Name = dto.Name;
            existingProduct.Slug = SlugHelper.GenerateSlug(dto.Name);
            existingProduct.Description = dto.Description;
            existingProduct.Price = dto.Price;
            existingProduct.Cost = dto.Cost;
            existingProduct.CategoryId = dto.CategoryId;
            existingProduct.BrandId = dto.BrandId;
            existingProduct.IsPopular = dto.IsPopular;
            existingProduct.Status = dto.Status;
            existingProduct.UpdatedAt = DateTime.UtcNow;

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

        private ProductDto MapWithBranchStock(Product p, int? branchId)
        {
            var dto = _mapper.Map<ProductDto>(p);

            IEnumerable<Stock> relevantStocks = p.Variants.Any(v => v.IsActive)
                ? p.Variants.Where(v => v.IsActive).SelectMany(v => v.Stocks)
                : p.Stocks.Where(s => s.ProductVariantId == null);

            if (branchId.HasValue)
                relevantStocks = relevantStocks.Where(s => s.BranchId == branchId.Value);

            var stocksList = relevantStocks.ToList();

            dto.TotalStock = stocksList.Sum(s => s.Quantity);
            var minLevel = stocksList.Any() ? stocksList.Max(s => s.MinimumStockLevel) : 0;

            dto.StockStatus = dto.TotalStock <= 0
                ? StockStatus.OutOfStock
                : dto.TotalStock <= minLevel
                    ? StockStatus.LowStock
                    : StockStatus.InStock;

            return dto;
        }

        private ProductDto ApplyTranslation(ProductDto dto, Product product, LanguageCode? languageCode)
        {
            if (languageCode == null || languageCode == LanguageCode.en)
                return dto;

            var langStr = languageCode.Value.ToString();

            var translation = product.Translations
                .FirstOrDefault(t => t.LanguageCode.Equals(langStr) && !t.IsDeleted);

            if (translation != null && !string.IsNullOrEmpty(translation.Name))
                dto.Name = translation.Name;

            return dto;
        }

        private ProductDetailDto ApplyTranslationToDetail(ProductDetailDto dto, Product product, LanguageCode? languageCode)
        {
            if (languageCode == null || languageCode == LanguageCode.en)
                return dto;

            var langStr = languageCode.Value.ToString();

            var translation = product.Translations
                .FirstOrDefault(t => t.LanguageCode.Equals(langStr) && !t.IsDeleted);

            if (translation != null)
            {
                if (!string.IsNullOrEmpty(translation.Name))
                    dto.Name = translation.Name;
                if (!string.IsNullOrEmpty(translation.Description))
                    dto.Description = translation.Description;
            }

            return dto;
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