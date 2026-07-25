using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.DTOs.Cart;
using Onpoint.Store.Application.Services.CartServ;
using System.Security.Claims;

[Route("api/cart")]
[ApiController]
[Authorize(Roles = "SuperAdmin")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;
    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    private int GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var userId))
            throw new UnauthorizedAccessException("Invalid or missing user identifier in token.");
        return userId;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyCart(CancellationToken ct = default)
    {
        var userId = GetUserId();
        var result = await _cartService.GetUserCartAsync(userId, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpPost("items/{productid}")]
    public async Task<IActionResult> AddToCart(int productid, [FromBody] AddToCartDto dto, CancellationToken ct = default)
    {
        var userId = GetUserId();
        var result = await _cartService.AddToCartAsync(userId, productid, dto, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpPut("items/{cartItemId}")]
    public async Task<IActionResult> UpdateQuantity(int cartItemId, [FromBody] UpdateCartItemDto dto, CancellationToken ct = default)
    {
        var userId = GetUserId();
        var result = await _cartService.UpdateItemQuantityAsync(userId, cartItemId, dto, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpDelete("items/{cartItemId}")]
    public async Task<IActionResult> RemoveItem(int cartItemId, CancellationToken ct = default)
    {
        var userId = GetUserId();
        var result = await _cartService.RemoveItemAsync(userId, cartItemId, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpDelete]
    public async Task<IActionResult> ClearMyCart(CancellationToken ct = default)
    {
        var userId = GetUserId();
        var result = await _cartService.ClearCartAsync(userId, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpPost("coupon")]
    public async Task<IActionResult> ApplyCoupon([FromBody] ApplyCouponDto dto, CancellationToken ct = default)
    {
        var userId = GetUserId();
        var result = await _cartService.ApplyCouponAsync(userId, dto, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpDelete("coupon")]
    public async Task<IActionResult> RemoveCoupon(CancellationToken ct = default)
    {
        var userId = GetUserId();
        var result = await _cartService.RemoveCouponAsync(userId, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }
}