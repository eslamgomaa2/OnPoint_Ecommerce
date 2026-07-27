using BuildingBlocks.Results;
using Onpoint.Store.Application.DTOs.Cart;
using Onpoint.Store.Application.DTOs.ProductVariant;
using Onpoint.Store.Application.DTOs.Wishlist;

public interface IWishlistService
{
    Task<ServiceResult<List<WishlistItemDto>>> GetUserWishlistAsync(int userId, CancellationToken ct = default);

    Task<ServiceResult<WishlistItemDto>> AddToWishlistAsync(int userId, int productId, int? productVariantId, CancellationToken ct = default);

    Task<ServiceResult<string>> RemoveFromWishlistAsync(int userId, int productId, int? productVariantId, CancellationToken ct = default);
    Task<ServiceResult<ProductVariantDto>> SelectWishlistVariantAsync(int userId, int productId, int productVariantId, CancellationToken ct = default);
    Task<ServiceResult<CartDto>> MoveToCartAsync(int userId, int productId, int? productVariantId, CancellationToken ct = default);
}