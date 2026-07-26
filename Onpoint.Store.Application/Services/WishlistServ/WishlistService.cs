using AutoMapper;
using BuildingBlocks.Results;
using FluentValidation;
using Onpoint.Store.Application.DTOs.Cart;
using Onpoint.Store.Application.DTOs.Wishlist;
using Onpoint.Store.Application.Services.CartServ;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Repositories;

namespace Onpoint.Store.Application.Services.WishlistServ
{
    public class WishlistService : IWishlistService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ServiceResultHandler _resultHandler;
        private readonly IValidator<AddToWishlistDto> _addValidator;
        private readonly ICartService _cartService;

        public WishlistService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ServiceResultHandler resultHandler,
            IValidator<AddToWishlistDto> addValidator,
            ICartService cartService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _resultHandler = resultHandler;
            _addValidator = addValidator;
            _cartService = cartService;
        }

        public async Task<ServiceResult<List<WishlistItemDto>>> GetUserWishlistAsync(int userId, CancellationToken ct = default)
        {
            var items = await _unitOfWork.Wishlists.GetUserWishlistAsync(userId, ct);
            var dto = _mapper.Map<List<WishlistItemDto>>(items);
            return _resultHandler.Success(dto);
        }

        public async Task<ServiceResult<WishlistItemDto>> AddToWishlistAsync(
     int userId, int productId, int? productVariantId, CancellationToken ct = default)
        {
            var product = await _unitOfWork.Products.GetByIdWithVariantsAsync(productId, ct);
            if (product == null || product.IsDeleted)
                return _resultHandler.NotFound<WishlistItemDto>("Product not found.");


            if (product.Variants.Any(v => v.IsActive))
            {
                if (!productVariantId.HasValue)
                    return _resultHandler.BadRequest<WishlistItemDto>("This product requires selecting a variant.");

                var variant = product.Variants.FirstOrDefault(v => v.Id == productVariantId.Value && v.IsActive);
                if (variant == null)
                    return _resultHandler.BadRequest<WishlistItemDto>("Selected variant is not available.");
            }

            var existing = await _unitOfWork.Wishlists.GetByUserAndProductAsync(userId, productId, productVariantId, ct);
            if (existing != null)
                return _resultHandler.BadRequest<WishlistItemDto>("Product is already in your wishlist.");

            var wishlistItem = new Wishlist
            {
                UserId = userId,
                ProductId = productId,
                ProductVariantId = productVariantId
            };

            await _unitOfWork.Wishlists.AddAsync(wishlistItem, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var saved = await _unitOfWork.Wishlists.GetByUserAndProductAsync(userId, productId, productVariantId, ct);
            return _resultHandler.Created(_mapper.Map<WishlistItemDto>(saved));
        }

        public async Task<ServiceResult<string>> RemoveFromWishlistAsync(int userId, int productId, int? productVariantId, CancellationToken ct = default)
        {
            var item = await _unitOfWork.Wishlists.GetByUserAndProductAsync(userId, productId, productVariantId, ct);
            if (item == null)
                return _resultHandler.NotFound<string>("Item not found in wishlist.");

            _unitOfWork.Wishlists.Remove(item);
            await _unitOfWork.SaveChangesAsync(ct);

            return _resultHandler.Deleted<string>("Item removed from wishlist.");
        }

        public async Task<ServiceResult<CartDto>> MoveToCartAsync(int userId, int productId, int? productVariantId, CancellationToken ct = default)
        {
            var wishlistItem = await _unitOfWork.Wishlists.GetByUserAndProductAsync(userId, productId, productVariantId, ct);
            if (wishlistItem == null)
                return _resultHandler.NotFound<CartDto>("Item not found in wishlist.");

            var product = await _unitOfWork.Products.GetByIdAsync(productId, ct);
            if (product == null || product.IsDeleted || !product.IsActive)
                return _resultHandler.BadRequest<CartDto>("This product is no longer available.");

            var addResult = await _cartService.AddToCartAsync(userId, productId, new AddToCartDto
            {
                ProductVariantId = wishlistItem.ProductVariantId,
                Quantity = 1
            }, ct);

            if (!addResult.Succeeded)
                return addResult;

            _unitOfWork.Wishlists.Remove(wishlistItem);
            await _unitOfWork.SaveChangesAsync(ct);

            return addResult;
        }
    }
}
