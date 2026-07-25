using BuildingBlocks.Results;
using Onpoint.Store.Application.DTOs.Cart;
using Onpoint.Store.Application.DTOs.Wishlist;

namespace Onpoint.Store.Application.Services.WishlistServ
{
    public interface IWishlistService
    {
        Task<ServiceResult<List<WishlistItemDto>>> GetUserWishlistAsync(int userId, CancellationToken ct = default);
        Task<ServiceResult<WishlistItemDto>> AddToWishlistAsync(int userId, int Productid, CancellationToken ct = default);
        Task<ServiceResult<string>> RemoveFromWishlistAsync(int userId, int productId, CancellationToken ct = default);
        Task<ServiceResult<CartDto>> MoveToCartAsync(int userId, int productId, CancellationToken ct = default);
    }
}
