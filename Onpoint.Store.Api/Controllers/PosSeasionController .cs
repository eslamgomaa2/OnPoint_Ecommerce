using BuildingBlocks.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.DTOs.Customer;
using Onpoint.Store.Application.DTOs.Pos;
using Onpoint.Store.Application.DTOs.PosSession;
using Onpoint.Store.Application.Services.PosServ;
using System.Security.Claims;

namespace Onpoint.Store.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Cashier,SuperAdmin,BranchManager")]
    public class PosSessionController : ControllerBase
    {
        private readonly IPosSessionService _posSessionService;

        public PosSessionController(IPosSessionService posSessionService)
        {
            _posSessionService = posSessionService;
        }


        private (int UserId, int? BranchId) GetUserAndBranchId()
        {
            var userClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            var branchClaim = User.FindFirstValue("BranchId");

            if (string.IsNullOrEmpty(userClaim) || !int.TryParse(userClaim, out var userId))
                throw new UnauthorizedAccessException("Invalid or missing user identifier in token.");

            int? branchId = int.TryParse(branchClaim, out var bId) ? bId : null;

            return (userId, branchId);
        }


        [HttpPost("create")]
        public async Task<ActionResult<ServiceResult<PosSessionDto>>> CreateSession()
        {
            var (userId, branchId) = GetUserAndBranchId();

            if (!branchId.HasValue)
                return BadRequest(new ServiceResult<PosSessionDto>
                {

                    Message = "Cashier is not assigned to a branch."
                });

            var result = await _posSessionService.CreateSessionAsync(userId, branchId.Value);
            return StatusCode((int)result.HttpStatusCode, result);
        }


        [HttpGet("{sessionId}")]
        public async Task<ActionResult<ServiceResult<PosSessionDto>>> GetSession(int sessionId)
        {
            var result = await _posSessionService.GetSessionAsync(sessionId);
            return StatusCode((int)result.HttpStatusCode, result);
        }


        [HttpGet("active")]
        public async Task<ActionResult<ServiceResult<PosSessionDto>>> GetActiveSession()
        {
            var (userId, _) = GetUserAndBranchId();
            var result = await _posSessionService.GetActiveSessionForCashierAsync(userId);
            return StatusCode((int)result.HttpStatusCode, result);
        }


        [HttpPost("{sessionId}/hold")]
        public async Task<ActionResult<ServiceResult<bool>>> HoldSession(int sessionId)
        {
            var result = await _posSessionService.HoldSessionAsync(sessionId);
            return StatusCode((int)result.HttpStatusCode, result);
        }


        [HttpPost("{sessionId}/resume")]
        public async Task<ActionResult<ServiceResult<PosSessionDto>>> ResumeSession(int sessionId)
        {
            var (userId, _) = GetUserAndBranchId();
            var result = await _posSessionService.ResumeSessionAsync(sessionId, userId);
            return StatusCode((int)result.HttpStatusCode, result);
        }


        [HttpPost("{sessionId}/cancel")]
        public async Task<ActionResult<ServiceResult<bool>>> CancelSession(int sessionId)
        {
            var result = await _posSessionService.CancelSessionAsync(sessionId);
            return StatusCode((int)result.HttpStatusCode, result);
        }


        [HttpPost("{sessionId}/clear")]
        public async Task<ActionResult<ServiceResult<PosSessionDto>>> ClearSession(int sessionId)
        {
            var result = await _posSessionService.ClearSessionAsync(sessionId);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        // ==================== ITEM MANAGEMENT ====================


        [HttpPost("scan")]
        public async Task<ActionResult<ServiceResult<PosSessionDto>>> ScanItem([FromBody] ScanPosItemDto dto)
        {
            var result = await _posSessionService.ScanAndAddItemAsync(dto);
            return StatusCode((int)result.HttpStatusCode, result);
        }


        [HttpPost("Add/items")]
        public async Task<ActionResult<ServiceResult<PosSessionDto>>> AddItem([FromQuery] int sessionId, [FromQuery] int productId, [FromQuery] int? productVariantId, [FromQuery] int quantity = 1)
        {
            var result = await _posSessionService.AddItemAsync(sessionId, productId, productVariantId, quantity);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpDelete("{sessionId}/items/{itemId}")]
        public async Task<ActionResult<ServiceResult<PosSessionDto>>> RemoveItem(int sessionId, int itemId)
        {
            var result = await _posSessionService.RemoveItemAsync(sessionId, itemId);
            return StatusCode((int)result.HttpStatusCode, result);
        }


        [HttpPut("{sessionId}/items/{itemId}/quantity")]
        public async Task<ActionResult<ServiceResult<PosSessionDto>>> UpdateItemQuantity(
            int sessionId,
            int itemId,
            [FromBody] UpdatePosSessionItemDto dto)
        {
            var result = await _posSessionService.UpdateItemQuantityAsync(sessionId, itemId, dto.Quantity);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        // ==================== CUSTOMER MANAGEMENT ====================


        [HttpPost("AssignCustomer/{sessionId}")]
        public async Task<ActionResult<ServiceResult<PosSessionDto>>> AssignCustomer(int sessionId, [FromBody] CreateCustomerDto dto)
        {
            var result = await _posSessionService.AssignCustomerToSessionAsync(sessionId, dto);
            return StatusCode((int)result.HttpStatusCode, result);
        }





        // ==================== COUPON MANAGEMENT ====================


        [HttpPost("{sessionId}/coupon")]
        public async Task<ActionResult<ServiceResult<PosSessionDto>>> ApplyCoupon(
            int sessionId,
            [FromBody] string couponCode)
        {
            var result = await _posSessionService.ApplyCouponAsync(sessionId, couponCode);
            return StatusCode((int)result.HttpStatusCode, result);
        }


        [HttpDelete("{sessionId}/coupon")]
        public async Task<ActionResult<ServiceResult<PosSessionDto>>> RemoveCoupon(int sessionId)
        {
            var result = await _posSessionService.RemoveCouponAsync(sessionId);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        // ==================== PAYMENT & COMPLETION ====================


        [HttpGet("{sessionId}/receipt-preview")]
        public async Task<ActionResult<ServiceResult<ReceiptPreviewDto>>> PreviewReceipt(int sessionId)
        {
            var result = await _posSessionService.PreviewReceiptAsync(sessionId);
            return StatusCode((int)result.HttpStatusCode, result);
        }


        [HttpPost("{sessionId}/complete")]
        public async Task<IActionResult> CompleteSession(int sessionId, [FromBody] CompletePosSessionDto dto)
        {
            var result = await _posSessionService.CompleteSessionAsync(sessionId, dto);
            return StatusCode((int)result.HttpStatusCode, result);
        }
    }
}