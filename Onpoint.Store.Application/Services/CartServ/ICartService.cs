using BuildingBlocks.Results;
using Onpoint.Store.Application.DTOs.Cart;

namespace Onpoint.Store.Application.Services.CartServ
{
    public interface ICartService
    {
        Task<ServiceResult<CartDto>> GetUserCartAsync(int userId, CancellationToken ct = default);
        Task<ServiceResult<CartDto>> AddToCartAsync(int userId, int productid, AddToCartDto dto, CancellationToken ct = default);
        Task<ServiceResult<CartDto>> UpdateItemQuantityAsync(int userId, int cartItemId, UpdateCartItemDto dto, CancellationToken ct = default);
        Task<ServiceResult<string>> RemoveItemAsync(int userId, int cartItemId, CancellationToken ct = default);
        Task<ServiceResult<string>> ClearCartAsync(int userId, CancellationToken ct = default);
        Task<ServiceResult<CartDto>> ApplyCouponAsync(int userId, ApplyCouponDto dto, CancellationToken ct = default);
        Task<ServiceResult<CartDto>> RemoveCouponAsync(int userId, CancellationToken ct = default);
    }
}