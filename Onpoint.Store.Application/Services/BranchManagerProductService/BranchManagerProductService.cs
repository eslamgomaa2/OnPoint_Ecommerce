using AutoMapper;
using BuildingBlocks.Results;
using FluentValidation;
using Onpoint.Store.Application.DTOs.Media;
using Onpoint.Store.Application.DTOs.Product;
using Onpoint.Store.Application.DTOs.Product.BranchManger;
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

        public BranchManagerProductService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ServiceResultHandler resultHandler,
            IValidator<CreateProductByBranchManagerDto> createValidator,
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
        // DASHBOARD
        // ============================================================================
        public async Task<ServiceResult<ProductDashboardDto>> GetDashboardCountsAsync(int branchId, CancellationToken ct = default)
        {
            var inStock = await _unitOfWork.Stocks.GetInStockCountAsync(branchId, ct);
            var lowStock = await _unitOfWork.Stocks.GetLowStockCountAsync(branchId, ct);
            var outOfStock = await _unitOfWork.Stocks.GetOutOfStockCountAsync(branchId, ct);

            return _resultHandler.Success(new ProductDashboardDto
            {
                TotalProducts = inStock + lowStock + outOfStock,
                InStock = inStock,
                LowStock = lowStock,
                OutOfStock = outOfStock
            });
        }

        // ============================================================================
        // GET PAGED
        // ============================================================================
        public async Task<ServiceResult<PagedResult<ProductDto>>> GetFilteredPagedAsync(
            int branchId,
            PaginationRequest request,
            int? categoryId = null,
            string? searchTerm = null,
            LanguageCode? languageCode = null,
            CancellationToken ct = default)
        {
            var (items, totalCount) = await _unitOfWork.Products.GetFilteredPagedAsync(
                categoryId, searchTerm, branchId, request.PageNumber, request.PageSize, ct);

            var dtoItems = items
                .Select(p => ApplyTranslation(MapWithBranchStock(p, branchId), p, languageCode))
                .ToList();

            var pagedResult = PagedResult<ProductDto>.Create(dtoItems, totalCount, request.PageNumber, request.PageSize);
            return _resultHandler.Success(pagedResult);
        }

        // ============================================================================
        // GET BY ID
        // ============================================================================
        public async Task<ServiceResult<ProductDetailDto>> GetByIdAsync(
            int branchId,
            int id,
            LanguageCode? languageCode = null,
            CancellationToken ct = default)
        {
            var product = await _unitOfWork.Products.GetWithDetailsAsync(id, ct);
            if (product is null)
                return _resultHandler.NotFound<ProductDetailDto>("Product not found");

            if (!HasStockInBranch(product, branchId))
                return _resultHandler.NotFound<ProductDetailDto>("Product not available in this branch");

            var dto = _mapper.Map<ProductDetailDto>(product);

            var baseDto = MapWithBranchStock(product, branchId);
            dto.TotalStock = baseDto.TotalStock;
            dto.StockStatus = baseDto.StockStatus;



            ApplyTranslationToDetail(dto, product, languageCode);

            return _resultHandler.Success(dto);
        }

        // ============================================================================
        // CREATE
        // ============================================================================
        public async Task<ServiceResult<ProductDto>> CreateAsync(int branchId, CreateProductByBranchManagerDto dto, CancellationToken ct = default)
        {
            var validationResult = await _createValidator.ValidateAsync(dto, ct);
            if (!validationResult.IsValid)
                throw new FluentValidation.ValidationException(validationResult.Errors);

            var branchExists = await _unitOfWork.Branches.GetByIdAsync(branchId, ct);
            if (branchExists is null)
                return _resultHandler.NotFound<ProductDto>("Branch not found");

            var product = _mapper.Map<Product>(dto);

            product.Slug = await GenerateUniqueSlugAsync(product.Name, ct);
            product.Status = dto.Status;

            // Images
            if (dto.Images?.Any() == true)
            {
                product.Images.Clear();
                foreach (var img in dto.Images)
                    product.Images.Add(new ProductImage { ImageUrl = img.ImageUrl, IsPrimary = img.IsPrimary });
            }

            // Discount
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

            // Variants (REQUIRED)
            var variantTracker = new List<(CreateProductVariantByBranchManagerDto Dto, ProductVariant Entity)>();
            product.Variants.Clear();

            foreach (var v in dto.Variants)
            {
                var variant = await BuildVariantAsync(product.Id, v, dto.Name, dto.CategoryId, ct);
                if (variant is null)
                    return _resultHandler.BadRequest<ProductDto>("Failed to create variant.");

                product.Variants.Add(variant);
                variantTracker.Add((v, variant));
            }

            await _unitOfWork.Products.AddAsync(product, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            // Add stocks to variants
            foreach (var (vDto, savedVariant) in variantTracker)
            {
                AddBranchStocks(savedVariant, vDto.BranchStocks, branchId, dto.MinimumStockLevel);
            }

            await _unitOfWork.SaveChangesAsync(ct);

            return _resultHandler.Created(_mapper.Map<ProductDto>(product));
        }

        // ============================================================================
        // UPDATE
        // ============================================================================
        public async Task<ServiceResult<ProductDto>> UpdateAsync(
            int branchId,
            int id,
            UpdateProductDto dto,
            CancellationToken ct = default)
        {
            var validationResult = await _updateValidator.ValidateAsync(dto, ct);
            if (!validationResult.IsValid)
                throw new FluentValidation.ValidationException(validationResult.Errors);

            var existingProduct = await _unitOfWork.Products.GetWithStocksForBranchCheckAsync(id, ct);
            if (existingProduct is null)
                return _resultHandler.NotFound<ProductDto>("Product not found to update");

            if (!HasStockInBranch(existingProduct, branchId))
                return _resultHandler.BadRequest<ProductDto>("Product not available in this branch");

            // Update basic info
            existingProduct.Name = dto.Name;
            if (dto.Name != existingProduct.Name)
                existingProduct.Slug = await GenerateUniqueSlugAsync(dto.Name, ct);

            existingProduct.Description = dto.Description;
            existingProduct.CategoryId = dto.CategoryId;
            existingProduct.BrandId = dto.BrandId;
            existingProduct.IsPopular = dto.IsPopular;
            existingProduct.Status = dto.Status;
            existingProduct.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Products.Update(existingProduct);
            await _unitOfWork.SaveChangesAsync(ct);

            var updated = await _unitOfWork.Products.GetWithDetailsAsync(id, ct);
            return _resultHandler.Success(_mapper.Map<ProductDto>(updated ?? existingProduct));
        }

        // ============================================================================
        // DELETE
        // ============================================================================
        public async Task<ServiceResult<string>> DeleteAsync(int branchId, int id, CancellationToken ct = default)
        {
            var product = await _unitOfWork.Products.GetWithStocksForBranchCheckAsync(id, ct);
            if (product is null)
                return _resultHandler.NotFound<string>("Product not found to delete");

            if (!HasStockInBranch(product, branchId))
                return _resultHandler.BadRequest<string>("Product not available in this branch");

            product.IsDeleted = true;
            product.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Products.Update(product);
            await _unitOfWork.SaveChangesAsync(ct);

            return _resultHandler.Deleted<string>();
        }

        // ============================================================================
        // PRIVATE HELPERS
        // ============================================================================

        private static bool HasStockInBranch(Product product, int branchId)
        {
            return product.Variants.Any(v => v.IsActive && v.Stocks.Any(s => s.BranchId == branchId));
        }

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

        private async Task<ProductVariant?> BuildVariantAsync(int productId, CreateProductVariantByBranchManagerDto dto, string productName, int categoryId, CancellationToken ct)
        {
            var sku = dto.SkuMode == CodeGenerationMode.Manual
                ? dto.Sku!
                : await _skuGeneratorService.GenerateUniqueSkuAsync($"{productName}-VAR", categoryId);

            if (dto.SkuMode == CodeGenerationMode.Manual && await _unitOfWork.ProductVariants.SkuExistsAsync(sku, null, ct))
                return null;

            var variant = new ProductVariant
            {
                ProductId = productId,
                Price = dto.Price,
                IsActive = true,
                Sku = sku
            };

            // Barcode
            if (dto.BarcodeMode is not null)
            {
                variant.Barcode = dto.BarcodeMode == CodeGenerationMode.Manual ? dto.Barcode : _barcodeService.GenerateValue();
                await GenerateBarcodeImageAsync(variant, ct);
            }

            // QR Code
            if (dto.QrCodeMode is not null)
            {
                variant.QrCodeValue = dto.QrCodeMode == CodeGenerationMode.Manual ? dto.QrCodeValue : _qrCodeService.GenerateValue(variant.Sku);
                await GenerateQrCodeImageAsync(variant, ct);
            }

            // Attributes
            if (dto.Attributes?.Any() == true)
            {
                foreach (var a in dto.Attributes)
                {
                    variant.AttributeValues.Add(new VariantAttributeValue
                    {
                        ProductAttributeId = a.ProductAttributeId,
                        Value = a.Value
                    });
                }
            }

            return variant;
        }

        private async Task GenerateBarcodeImageAsync(ProductVariant variant, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(variant.Barcode)) return;

            byte[] bytes = _barcodeService.GenerateImage(variant.Barcode);
            using var stream = new MemoryStream(bytes);

            var upload = await _mediaService.UploadProductImageAsync(new FileUploadDto
            {
                FileName = $"barcode_var_{variant.Sku}.png",
                FileContent = stream
            }, ct);

            if (upload.Succeeded)
                variant.BarcodeImagePath = upload.Data;
        }

        private async Task GenerateQrCodeImageAsync(ProductVariant variant, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(variant.QrCodeValue)) return;

            byte[] bytes = _qrCodeService.GenerateImage(variant.QrCodeValue);
            using var stream = new MemoryStream(bytes);

            var upload = await _mediaService.UploadProductImageAsync(new FileUploadDto
            {
                FileName = $"qrcode_var_{variant.Sku}.png",
                FileContent = stream
            }, ct);

            if (upload.Succeeded)
                variant.QrCodeImagePath = upload.Data;
        }

        private static void AddBranchStocks(ProductVariant variant, List<BranchManagerStockDto>? branchStocks, int branchId, int minimumStockLevel)
        {
            if (branchStocks?.Any() != true) return;

            foreach (var bs in branchStocks)
            {
                variant.Stocks.Add(new Stock
                {
                    ProductId = variant.ProductId,
                    ProductVariantId = variant.Id,
                    BranchId = branchId,
                    Quantity = bs.Quantity,
                    ReservedQuantity = 0,
                    MinimumStockLevel = bs.MinimumStockLevel != 0 ? bs.MinimumStockLevel : minimumStockLevel
                });
            }
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
    }
}