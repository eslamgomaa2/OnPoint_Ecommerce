using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.DTOs.Cart;
using Onpoint.Store.Application.Services.CartServ;
using System.Security.Claims;

namespace Onpoint.Store.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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

        [HttpGet("[action]")]
        public async Task<IActionResult> GetMyCart(CancellationToken ct = default)
        {
            var userId = GetUserId();
            var result = await _cartService.GetUserCartAsync(userId, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDto dto, CancellationToken ct = default)
        {
            var userId = GetUserId();
            var result = await _cartService.AddToCartAsync(userId, dto, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateQuantity([FromBody] UpdateCartItemDto dto, CancellationToken ct = default)
        {
            var userId = GetUserId();
            var result = await _cartService.UpdateItemQuantityAsync(userId, dto, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpDelete("item/{cartItemId}")]
        public async Task<IActionResult> RemoveItem(int cartItemId, CancellationToken ct = default)
        {
            var userId = GetUserId();
            var result = await _cartService.RemoveItemAsync(userId, cartItemId, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearMyCart(CancellationToken ct = default)
        {
            var userId = GetUserId();
            var result = await _cartService.ClearCartAsync(userId, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }


        [HttpPost("coupon/apply")]
        public async Task<IActionResult> ApplyCoupon([FromBody] ApplyCouponDto dto, CancellationToken ct = default)
        {
            var userId = GetUserId();
            var result = await _cartService.ApplyCouponAsync(userId, dto, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpDelete("coupon/remove")]
        public async Task<IActionResult> RemoveCoupon(CancellationToken ct = default)
        {
            var userId = GetUserId();
            var result = await _cartService.RemoveCouponAsync(userId, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }
    }
}