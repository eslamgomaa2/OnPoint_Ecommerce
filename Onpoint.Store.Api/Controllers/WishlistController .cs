using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Onpoint.Store.Api.Controllers
{
    [Route("api/wishlist")]
    [ApiController]
    [Authorize]
    public class WishlistController : ControllerBase
    {
        private readonly IWishlistService _wishlistService;

        public WishlistController(IWishlistService wishlistService)
        {
            _wishlistService = wishlistService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyWishlist(CancellationToken ct = default)
        {
            var userId = GetUserId();
            var result = await _wishlistService.GetUserWishlistAsync(userId, ct);
            return ToResponse(result);
        }


        [HttpPost("products/{productId:int}")]
        public async Task<IActionResult> AddToWishlist(
            int productId,
            CancellationToken ct = default)
        {
            var userId = GetUserId();
            var result = await _wishlistService.AddToWishlistAsync(userId, productId, null, ct);
            return ToResponse(result);
        }


        [HttpDelete("products/{productId:int}")]
        public async Task<IActionResult> RemoveFromWishlist(
            int productId,
            CancellationToken ct = default)
        {
            var userId = GetUserId();
            var result = await _wishlistService.RemoveFromWishlistAsync(userId, productId, null, ct);
            return ToResponse(result);
        }

        [HttpPatch("products/{productId:int}/variant")]
        public async Task<IActionResult> SelectWishlistVariant(int productId, [FromQuery] int variantId, CancellationToken ct = default)
        {
            var userId = GetUserId();
            var result = await _wishlistService.SelectWishlistVariantAsync(userId, productId, variantId, ct);
            return ToResponse(result);
        }
        [HttpPost("products/{productId:int}/move-to-cart")]
        public async Task<IActionResult> MoveToCart(int productId, CancellationToken ct = default)
        {
            var userId = GetUserId();
            var result = await _wishlistService.MoveToCartAsync(userId, productId, null, ct);
            return ToResponse(result);
        }

        private int GetUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var userId))
                throw new UnauthorizedAccessException("Invalid or missing user identifier in token.");

            return userId;
        }

        private IActionResult ToResponse<T>(BuildingBlocks.Results.ServiceResult<T> result)
            => StatusCode((int)result.HttpStatusCode, result);
    }
}