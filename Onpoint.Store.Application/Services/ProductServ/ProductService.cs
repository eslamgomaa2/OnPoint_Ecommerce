using AutoMapper;
using BuildingBlocks.Results;
using FluentValidation;
using Onpoint.Store.Application.DTOs;
using Onpoint.Store.Application.DTOs.Media;
using Onpoint.Store.Application.DTOs.Product;
using Onpoint.Store.Application.DTOs.ProductVariant;
using Onpoint.Store.Application.Helpers;
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

        public ProductService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ServiceResultHandler resultHandler,
            IValidator<CreateProductDto> createValidator,
            IValidator<UpdateProductDto> updateValidator,
            ISkuGeneratorService skuGenerator,
            IBarcodeService barcodeService,
            IQrCodeService qrCodeService,
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
            _mediaService = mediaService;
        }

        // ============================================================================
        // GET METHODS
        // ============================================================================

        public async Task<ServiceResult<ProductDto>> GetBySkuAsync(string sku, CancellationToken ct = default)
        {
            var product = await _unitOfWork.Products.GetBySkuAsync(sku, ct);
            if (product == null)
                return _resultHandler.NotFound<ProductDto>($"Product with SKU '{sku}' not found");

            return _resultHandler.Success(_mapper.Map<ProductDto>(product));
        }

        public async Task<ServiceResult<ProductDashboardDto>> GetDashboardCountsAsync(int? branchId = null, CancellationToken ct = default)
        {
            var (inStock, lowStock, outOfStock, total) = await _unitOfWork.Products.GetStockCountsAsync(branchId, ct);

            var dto = new ProductDashboardDto
            {
                TotalProducts = total,
                InStock = inStock,
                LowStock = lowStock,
                OutOfStock = outOfStock
            };

            return _resultHandler.Success(dto);
        }

        public async Task<ServiceResult<PagedResult<ProductDto>>> GetFilteredPagedAsync(
            PaginationRequest request, int? categoryId = null, string? searchTerm = null,
            int? branchId = null, LanguageCode? languageCode = null, int? currentUserId = null,
            CancellationToken ct = default)
        {
            var (items, totalCount) = await _unitOfWork.Products.GetFilteredPagedAsync(
                categoryId, searchTerm, branchId, request.PageNumber, request.PageSize, ct);

            var dtoItems = items.Select(p => ApplyTranslation(MapWithBranchStock(p, branchId), p, languageCode)).ToList();

            if (currentUserId.HasValue && dtoItems.Any())
            {
                var productIds = dtoItems.Select(d => d.Id).ToList();

                var wishlistedIds = await _unitOfWork.Wishlists.GetWishlistedProductIdsAsync(currentUserId.Value, productIds, ct);
                var cartedIds = await _unitOfWork.Carts.GetProductIdsInCartAsync(currentUserId.Value, productIds, ct);

                foreach (var dto in dtoItems)
                {
                    dto.IsInWishlist = wishlistedIds.Contains(dto.Id);
                    dto.IsInCart = cartedIds.Contains(dto.Id);
                }
            }

            var pagedResult = PagedResult<ProductDto>.Create(dtoItems, totalCount, request.PageNumber, request.PageSize);
            return _resultHandler.Success(pagedResult);
        }

        public async Task<ServiceResult<ProductDetailDto>> GetByIdAsync(
            int id, LanguageCode? languageCode = null, int? currentUserId = null, CancellationToken ct = default)
        {
            var product = await _unitOfWork.Products.GetWithDetailsAsync(id, ct);
            if (product is null)
                return _resultHandler.NotFound<ProductDetailDto>("Product not found");

            var dto = _mapper.Map<ProductDetailDto>(product);

            var baseDto = MapWithBranchStock(product, null);
            dto.TotalStock = baseDto.TotalStock;
            dto.StockStatus = baseDto.StockStatus;

            // ⚠️ All stocks are on variants
            var stocks = product.Variants
                .Where(v => v.IsActive)
                .SelectMany(v => v.Stocks);

            dto.BranchStock = stocks.Select(s => _mapper.Map<VariantStockDto>(s)).ToList();

            ApplyTranslationToDetail(dto, product, languageCode);

            if (currentUserId.HasValue)
            {
                var wishlistedIds = await _unitOfWork.Wishlists.GetWishlistedProductIdsAsync(currentUserId.Value, new[] { id }, ct);
                var cartedIds = await _unitOfWork.Carts.GetProductIdsInCartAsync(currentUserId.Value, new[] { id }, ct);

                dto.IsInWishlist = wishlistedIds.Contains(id);
                dto.IsInCart = cartedIds.Contains(id);
            }

            return _resultHandler.Success(dto);
        }

        // ============================================================================
        // CREATE
        // ============================================================================

        public async Task<ServiceResult<ProductDto>> CreateAsync(CreateProductDto dto, CancellationToken ct = default)
        {
            var validationResult = await _createValidator.ValidateAsync(dto, ct);
            if (!validationResult.IsValid)
                throw new FluentValidation.ValidationException(validationResult.Errors);

            var product = _mapper.Map<Product>(dto);

            product.Slug = await GenerateUniqueSlugAsync(product.Name, ct);
            product.Status = dto.Status;

            if (dto.Images?.Any() == true)
            {
                product.Images.Clear();
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

            var manualBarcodes = dto.Variants
                .Where(v => v.BarcodeMode == CodeGenerationMode.Manual && !string.IsNullOrEmpty(v.Barcode))
                .Select(v => v.Barcode!)
                .ToList();

            if (manualBarcodes.Any())
            {
                var duplicatesInRequest = manualBarcodes
                    .GroupBy(b => b)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicatesInRequest.Any())
                    return _resultHandler.BadRequest<ProductDto>(
                        $"الباركود مكرر داخل نفس الطلب: {string.Join(", ", duplicatesInRequest)}");

                var existingBarcodes = await _unitOfWork.ProductVariants
                    .GetExistingBarcodesAsync(manualBarcodes, ct);

                if (existingBarcodes.Any())
                    return _resultHandler.BadRequest<ProductDto>(
                        $"الباركود مستخدم بالفعل: {string.Join(", ", existingBarcodes)}");
            }

            var manualSkus = dto.Variants
                .Where(v => v.SkuMode == CodeGenerationMode.Manual && !string.IsNullOrEmpty(v.Sku))
                .Select(v => v.Sku!)
                .ToList();

            if (manualSkus.Any())
            {
                var duplicateSkusInRequest = manualSkus
                    .GroupBy(s => s)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicateSkusInRequest.Any())
                    return _resultHandler.BadRequest<ProductDto>(
                        $"الـ SKU مكرر داخل نفس الطلب: {string.Join(", ", duplicateSkusInRequest)}");

                foreach (var sku in manualSkus)
                {
                    if (await _unitOfWork.ProductVariants.SkuExistsAsync(sku, ct: ct))
                        return _resultHandler.BadRequest<ProductDto>($"الـ SKU '{sku}' مستخدم بالفعل.");
                }
            }

            var variantTracker = new List<(CreateProductVariantDto Dto, ProductVariant Entity)>();
            product.Variants.Clear();

            foreach (var v in dto.Variants)
            {
                var variant = new ProductVariant
                {
                    Price = v.Price,
                    Cost = v.Cost,
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

            if (dto.Shipping != null)
            {
                product.Shipping = new ProductShipping
                {
                    WeightKg = dto.Shipping.WeightKg,
                    LengthCm = dto.Shipping.LengthCm,
                    WidthCm = dto.Shipping.WidthCm,
                    HeightCm = dto.Shipping.HeightCm,
                    IsFragile = dto.Shipping.IsFragile,
                    IsHazardous = dto.Shipping.IsHazardous,
                    ShippingClass = dto.Shipping.ShippingClass
                };
            }

            await _unitOfWork.Products.AddAsync(product, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            foreach (var (vDto, savedVariant) in variantTracker)
            {
                if (vDto.BranchStocks?.Any() == true)
                {
                    foreach (var bs in vDto.BranchStocks)
                    {
                        savedVariant.Stocks.Add(new Stock
                        {
                            ProductId = product.Id,
                            ProductVariantId = savedVariant.Id,
                            BranchId = bs.BranchId,
                            Quantity = bs.Quantity,
                            ReservedQuantity = 0,
                            MinimumStockLevel = bs.MinimumStockLevel
                        });
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync(ct);

            var createdWithDetails = await _unitOfWork.Products.GetWithDetailsAsync(product.Id, ct);
            return _resultHandler.Created(_mapper.Map<ProductDto>(createdWithDetails ?? product));
        }

        // ============================================================================
        // UPDATE
        // ============================================================================

        public async Task<ServiceResult<ProductDto>> UpdateAsync(int id, UpdateProductDto dto, CancellationToken ct = default)
        {
            var validationResult = await _updateValidator.ValidateAsync(dto, ct);
            if (!validationResult.IsValid)
                throw new FluentValidation.ValidationException(validationResult.Errors);

            var existingProduct = await _unitOfWork.Products.GetWithFullDetailsForAdminAsync(id, includeDeleted: false, ct);
            if (existingProduct is null)
                return _resultHandler.NotFound<ProductDto>("Product not found to update");

            // Update basic info
            var originalName = existingProduct.Name;
            existingProduct.Name = dto.Name;

            if (dto.Name != originalName)
                existingProduct.Slug = await GenerateUniqueSlugAsync(dto.Name, ct);

            existingProduct.Description = dto.Description;
            existingProduct.CategoryId = dto.CategoryId;
            existingProduct.BrandId = dto.BrandId;
            existingProduct.IsPopular = dto.IsPopular;
            existingProduct.Status = dto.Status;
            existingProduct.UpdatedAt = DateTime.UtcNow;

            // Update Images (if provided)
            if (dto.Images != null)
            {
                existingProduct.Images.Clear();
                foreach (var img in dto.Images)
                {
                    existingProduct.Images.Add(new ProductImage
                    {
                        ImageUrl = img.ImageUrl,
                        IsPrimary = img.IsPrimary
                    });
                }
            }

            // Update Discount (if provided)
            if (dto.Discount != null)
            {
                existingProduct.Discounts.Clear();
                existingProduct.Discounts.Add(new Discount
                {
                    DiscountPercentage = dto.Discount.DiscountPercentage,
                    StartDate = dto.Discount.StartDate,
                    EndDate = dto.Discount.EndDate,
                    IsActive = true
                });
            }
            else if (dto.Discount == null && existingProduct.Discounts.Any())
            {
                existingProduct.Discounts.Clear();
            }


            if (dto.Variants != null)
            {
                var existingVariantIds = existingProduct.Variants.Select(v => v.Id).ToList();
                var updatedVariantIds = dto.Variants.Where(v => v.Id.HasValue).Select(v => v.Id!.Value).ToList();

                // Delete variants that are not in the update list
                var variantsToDelete = existingProduct.Variants
                    .Where(v => !updatedVariantIds.Contains(v.Id))
                    .ToList();

                foreach (var variantToDelete in variantsToDelete)
                {
                    variantToDelete.IsDeleted = true;
                    variantToDelete.IsActive = false;
                }

                // Update or create variants
                foreach (var variantDto in dto.Variants)
                {
                    if (variantDto.Id.HasValue && existingVariantIds.Contains(variantDto.Id.Value))
                    {
                        // Update existing variant
                        var existingVariant = existingProduct.Variants
                            .First(v => v.Id == variantDto.Id.Value);

                        existingVariant.Price = variantDto.Price;
                        existingVariant.Cost = variantDto.Cost;
                        existingVariant.IsActive = variantDto.IsActive;
                        existingVariant.UpdatedAt = DateTime.UtcNow;

                        // Update SKU if manual
                        if (variantDto.SkuMode == CodeGenerationMode.Manual && !string.IsNullOrEmpty(variantDto.Sku))
                            existingVariant.Sku = variantDto.Sku;

                        // Update Barcode if manual
                        if (variantDto.BarcodeMode == CodeGenerationMode.Manual && !string.IsNullOrEmpty(variantDto.Barcode))
                            existingVariant.Barcode = variantDto.Barcode;

                        // Update QR Code if manual
                        if (variantDto.QrCodeMode == CodeGenerationMode.Manual && !string.IsNullOrEmpty(variantDto.QrCodeValue))
                            existingVariant.QrCodeValue = variantDto.QrCodeValue;

                        // Update Attributes
                        if (variantDto.Attributes != null)
                        {
                            existingVariant.AttributeValues.Clear();
                            foreach (var attr in variantDto.Attributes)
                            {
                                existingVariant.AttributeValues.Add(new VariantAttributeValue
                                {
                                    ProductAttributeId = attr.ProductAttributeId,
                                    Value = attr.Value
                                });
                            }
                        }

                        if (variantDto.BranchStocks != null)
                        {
                            var stocksToRemove = existingVariant.Stocks.ToList();
                            foreach (var stock in stocksToRemove)
                            {
                                _unitOfWork.Stocks.Remove(stock);
                            }

                            foreach (var bs in variantDto.BranchStocks)
                            {
                                existingVariant.Stocks.Add(new Stock
                                {
                                    ProductId = existingProduct.Id,
                                    ProductVariantId = existingVariant.Id,
                                    BranchId = bs.BranchId,
                                    Quantity = bs.Quantity,
                                    ReservedQuantity = 0,
                                    MinimumStockLevel = bs.MinimumStockLevel
                                });
                            }
                        }
                    }
                    else
                    {
                        var newVariant = new ProductVariant
                        {
                            Price = variantDto.Price,
                            Cost = variantDto.Cost,
                            IsActive = variantDto.IsActive,
                            Sku = variantDto.SkuMode == CodeGenerationMode.Manual
                                ? variantDto.Sku!
                                : await _skuGeneratorService.GenerateUniqueSkuAsync($"{dto.Name}-VAR", dto.CategoryId)
                        };

                        newVariant.Barcode = variantDto.BarcodeMode == CodeGenerationMode.Manual
                            ? variantDto.Barcode
                            : _barcodeService.GenerateValue();

                        if (!string.IsNullOrEmpty(newVariant.Barcode))
                        {
                            byte[] barcodeBytes = _barcodeService.GenerateImage(newVariant.Barcode);
                            using var barcodeStream = new MemoryStream(barcodeBytes);
                            var barcodeUploadResult = await _mediaService.UploadProductImageAsync(new FileUploadDto
                            {
                                FileName = $"barcode_var_{newVariant.Sku}.png",
                                FileContent = barcodeStream
                            }, ct);
                            if (barcodeUploadResult.Succeeded)
                                newVariant.BarcodeImagePath = barcodeUploadResult.Data;
                        }

                        newVariant.QrCodeValue = variantDto.QrCodeMode == CodeGenerationMode.Manual
                            ? variantDto.QrCodeValue
                            : _qrCodeService.GenerateValue(newVariant.Sku);

                        if (!string.IsNullOrEmpty(newVariant.QrCodeValue))
                        {
                            byte[] qrCodeBytes = _qrCodeService.GenerateImage(newVariant.QrCodeValue);
                            using var qrStream = new MemoryStream(qrCodeBytes);
                            var qrUploadResult = await _mediaService.UploadProductImageAsync(new FileUploadDto
                            {
                                FileName = $"qrcode_var_{newVariant.Sku}.png",
                                FileContent = qrStream
                            }, ct);
                            if (qrUploadResult.Succeeded)
                                newVariant.QrCodeImagePath = qrUploadResult.Data;
                        }

                        // Attributes
                        if (variantDto.Attributes != null)
                        {
                            foreach (var attr in variantDto.Attributes)
                            {
                                newVariant.AttributeValues.Add(new VariantAttributeValue
                                {
                                    ProductAttributeId = attr.ProductAttributeId,
                                    Value = attr.Value
                                });
                            }
                        }

                        // Stocks
                        if (variantDto.BranchStocks != null)
                        {
                            foreach (var bs in variantDto.BranchStocks)
                            {
                                newVariant.Stocks.Add(new Stock
                                {
                                    ProductId = existingProduct.Id,
                                    ProductVariantId = newVariant.Id,
                                    BranchId = bs.BranchId,
                                    Quantity = bs.Quantity,
                                    ReservedQuantity = 0,
                                    MinimumStockLevel = bs.MinimumStockLevel
                                });
                            }
                        }

                        existingProduct.Variants.Add(newVariant);
                    }
                }
            }

            _unitOfWork.Products.Update(existingProduct);
            await _unitOfWork.SaveChangesAsync(ct);

            var updated = await _unitOfWork.Products.GetWithDetailsAsync(id, ct);
            return _resultHandler.Success(_mapper.Map<ProductDto>(updated ?? existingProduct));
        }
        // ============================================================================
        // DELETE
        // ============================================================================

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

        // ============================================================================
        // HELPERS
        // ============================================================================

        private async Task<string> GenerateUniqueSlugAsync(string name, CancellationToken ct)
        {
            var baseSlug = SlugHelper.GenerateSlug(name);
            var slug = baseSlug;
            int counter = 1;

            while (await _unitOfWork.Products.SlugExistsAsync(slug, ct))
            {
                slug = $"{baseSlug}-{counter}";
                counter++;
            }

            return slug;
        }

        private ProductDto MapWithBranchStock(Product p, int? branchId)
        {
            var dto = _mapper.Map<ProductDto>(p);


            IEnumerable<Stock> relevantStocks = p.Variants
                .Where(v => v.IsActive)
                .SelectMany(v => v.Stocks);

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


        public async Task<ServiceResult<PagedResult<ProductListItemDto>>> GetFilteredAsync(ProductFilterRequestDto filter, CancellationToken ct = default)
        {

            var (products, totalCount) = await _unitOfWork.Products.GetFilteredAsync(
                filter.CategoryId,
                filter.MinRating,
                filter.MinPrice,
                filter.MaxPrice,
                filter.InStockOnly,
                filter.Search,
                filter.SortBy,
                filter.PageNumber,
                filter.PageSize,
                ct);

            var dtos = products.Select(p => MapToListItem(p)).ToList();
            var result = PagedResult<ProductListItemDto>.Create(dtos, totalCount, filter.PageNumber, filter.PageSize);

            return _resultHandler.Success(result);
        }

        private ProductListItemDto MapToListItem(Product p)
        {
            var activeVariants = p.Variants.Where(v => v.IsActive).ToList();
            var minPrice = activeVariants.Any() ? activeVariants.Min(v => v.Price) : 0;

            var activeDiscount = p.Discounts
                .FirstOrDefault(d => d.IsActive && d.EndDate >= DateTime.UtcNow);
            var discountPercentage = activeDiscount?.DiscountPercentage;
            var originalPrice = discountPercentage.HasValue ? minPrice : (decimal?)null;
            var finalPrice = discountPercentage.HasValue
                ? minPrice * (1 - discountPercentage.Value / 100m)
                : minPrice;

            var inStock = activeVariants.Any(v =>
                v.Stocks != null && v.Stocks.Any(s => s.Quantity > s.MinimumStockLevel));

            var approvedReviews = p.Reviews.Count(r => r.IsApproved);
            var avgRating = approvedReviews > 0
                ? p.Reviews.Where(r => r.IsApproved).Average(r => r.Rating)
                : 0;

            var primaryImage = p.Images
                .OrderByDescending(i => i.IsPrimary)
                .FirstOrDefault();

            return new ProductListItemDto
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                Description = p.Description,
                PrimaryImageUrl = primaryImage?.ImageUrl,
                Price = finalPrice,
                OriginalPrice = originalPrice,
                DiscountPercentage = discountPercentage,
                AverageRating = avgRating,
                ReviewCount = approvedReviews,
                IsPopular = p.IsPopular,
                InStock = inStock,
                CategoryName = p.Category?.Name ?? string.Empty,
                BrandName = p.Brand?.Name,
                CreatedAt = p.CreatedAt,


                Variants = activeVariants.Select(v => new ProductVariantDto
                {
                    Id = v.Id,
                    ProductId = p.Id,
                    ProductName = p.Name,
                    Sku = v.Sku ?? string.Empty,
                    Barcode = v.Barcode,
                    BarcodeImagePath = v.BarcodeImagePath,
                    QrCodeValue = v.QrCodeValue,
                    QrCodeImagePath = v.QrCodeImagePath,
                    Price = v.Price,
                    Cost = v.Cost,
                    IsActive = v.IsActive,
                    Attributes = v.AttributeValues?.Select(av => new VariantAttributeValueDto
                    {
                        AttributeName = av.ProductAttribute?.Name ?? string.Empty,
                        Value = av.Value ?? string.Empty
                    }).ToList() ?? new List<VariantAttributeValueDto>()
                }).ToList()
            };
        }
    }
}
