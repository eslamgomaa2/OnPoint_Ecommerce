using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.Services.WishlistServ;
using System.Security.Claims;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Superdmin")]
public class WishlistController : ControllerBase
{
    private readonly IWishlistService _wishlistService;
    public WishlistController(IWishlistService wishlistService)
    {
        _wishlistService = wishlistService;
    }

    private int GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var userId))
            throw new UnauthorizedAccessException("Invalid or missing user identifier in token.");
        return userId;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyWishlist(CancellationToken ct = default)
    {
        var userId = GetUserId();
        var result = await _wishlistService.GetUserWishlistAsync(userId, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpPost("products/{productId}")]
    public async Task<IActionResult> AddToWishlist(int productId, CancellationToken ct = default)
    {
        var userId = GetUserId();
        var result = await _wishlistService.AddToWishlistAsync(userId, productId, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpDelete("products/{productId}")]
    public async Task<IActionResult> RemoveFromWishlist(int productId, CancellationToken ct = default)
    {
        var userId = GetUserId();
        var result = await _wishlistService.RemoveFromWishlistAsync(userId, productId, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpPost("products/{productId}/move-to-cart")]
    public async Task<IActionResult> MoveToCart(int productId, CancellationToken ct = default)
    {
        var userId = GetUserId();
        var result = await _wishlistService.MoveToCartAsync(userId, productId, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }
}