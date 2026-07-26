using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.DTOs.Refund;
using Onpoint.Store.Application.Interfaces;
using System.Security.Claims;

[ApiController]
[Route("api/orders/{orderId}/refunds")]
[Authorize(Roles = "SuperAdmin")]

public class RefundController : ControllerBase
{
    private readonly IRefundService _refundService;
    public RefundController(IRefundService refundService)
    {
        _refundService = refundService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RefundDto>>> GetRefunds(int orderId, CancellationToken ct)
    {
        var result = await _refundService.GetOrderRefundsAsync(orderId, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpPost("full")]
    public async Task<ActionResult<RefundDto>> FullRefund(int orderId, [FromBody] FullRefundRequestDto dto, CancellationToken ct)
    {
        var (userId, branchId) = GetUserAndBranchId();
        var result = await _refundService.CreateFullRefundAsync(userId, branchId, orderId, dto, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpPost("partial")]
    public async Task<ActionResult<RefundDto>> PartialRefund(int orderId, [FromBody] PartialRefundRequestDto dto, CancellationToken ct)
    {
        var (userId, branchId) = GetUserAndBranchId();
        var result = await _refundService.CreatePartialRefundAsync(userId, branchId, orderId, dto, ct);
        return StatusCode((int)result.HttpStatusCode, result);
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
}