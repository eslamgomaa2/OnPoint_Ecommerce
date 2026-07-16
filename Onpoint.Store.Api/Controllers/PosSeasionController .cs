using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.DTOs.PosSession;
using Onpoint.Store.Application.Services.PosServ;
using System.Security.Claims;

namespace Onpoint.Store.Api.Controllers
{
    [ApiController]
    [Route("api/pos")]
    [Authorize(Roles = "Cashier,BranchManager,SuperAdmin")]
    public class PosSeasionController : ControllerBase
    {
        private readonly IPosSessionService _posSessionService;

        public PosSeasionController(IPosSessionService posSessionService)
        {
            _posSessionService = posSessionService;
        }

        private int GetUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var userId))
                throw new UnauthorizedAccessException("Invalid or missing user identifier in token.");
            return userId;
        }
        [HttpPost("sessions")]
        public async Task<IActionResult> CreateSession()
        {
            var cashierId = GetUserId();
            var result = await _posSessionService.CreateSessionAsync(cashierId);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpGet("sessions/active")]
        public async Task<IActionResult> GetActiveSession()
        {
            var cashierId = GetUserId();
            var result = await _posSessionService.GetActiveSessionForCashierAsync(cashierId);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpGet("sessions/{sessionId}")]
        public async Task<IActionResult> GetSession(int sessionId)
        {
            var result = await _posSessionService.GetSessionAsync(sessionId);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpPost("sessions/{sessionId}/items")]
        public async Task<IActionResult> AddItem(int sessionId, [FromBody] AddPosSessionItemDto dto)
        {
            var result = await _posSessionService.AddItemAsync(sessionId, dto.ProductId, dto.Quantity);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpDelete("sessions/{sessionId}/items/{itemId}")]
        public async Task<IActionResult> RemoveItem(int sessionId, int itemId)
        {
            var result = await _posSessionService.RemoveItemAsync(sessionId, itemId);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpPut("sessions/{sessionId}/items/{itemId}")]
        public async Task<IActionResult> UpdateItemQuantity(int sessionId, int itemId, [FromBody] UpdatePosSessionItemDto dto)
        {
            var result = await _posSessionService.UpdateItemQuantityAsync(sessionId, itemId, dto.Quantity);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpPost("sessions/{sessionId}/apply-coupon")]
        public async Task<IActionResult> ApplyCoupon(int sessionId, [FromBody] string couponCode)
        {
            var result = await _posSessionService.ApplyCouponAsync(sessionId, couponCode);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpPost("sessions/{sessionId}/remove-coupon")]
        public async Task<IActionResult> RemoveCoupon(int sessionId)
        {
            var result = await _posSessionService.RemoveCouponAsync(sessionId);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpPost("sessions/{sessionId}/complete")]
        public async Task<IActionResult> CompleteSession(int sessionId, [FromBody] CompletePosSessionDto dto)
        {
            var result = await _posSessionService.CompleteSessionAsync(sessionId, dto.PaymentMethod);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpPost("sessions/{sessionId}/hold")]
        public async Task<IActionResult> HoldSession(int sessionId)
        {
            var result = await _posSessionService.HoldSessionAsync(sessionId);
            return StatusCode((int)result.HttpStatusCode, result);
        }
        [HttpPost("sessions/{sessionId}/resume")]
        public async Task<IActionResult> ResumeSession(int sessionId)
        {
            var cashierId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _posSessionService.ResumeSessionAsync(sessionId, cashierId);
            return StatusCode((int)result.HttpStatusCode, result);
        }
        [HttpPost("sessions/{sessionId}/cancel")]
        public async Task<IActionResult> CancelSession(int sessionId)
        {
            var result = await _posSessionService.CancelSessionAsync(sessionId);
            return StatusCode((int)result.HttpStatusCode, result);
        }
    }
}