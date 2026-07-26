using BuildingBlocks.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.DTOs.Order;
using Onpoint.Store.Application.Services.OrderServ;
using System.Security.Claims;
namespace Onpoint.Store.Api.Controllers
{
    [Route("api/dashboard")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin")]
    public class DashboardController : ControllerBase
    {
        private readonly IOrderService _orderService;
        public DashboardController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // GET /api/dashboard/orders
        [HttpGet("orders")]
        public async Task<ActionResult<ServiceResult<PagedResult<OrderListItemDto>>>> GetDashboardOrders(
            [FromQuery] GetOrdersQueryDto query, CancellationToken ct)
        {
            var (_, branchId) = GetUserAndBranchId();
            var result = await _orderService.GetDashboardPagedAsync(query, branchId, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        // GET /api/dashboard/orders/paged
        [HttpGet("orders/paged")]
        public async Task<IActionResult> GetOrdersPaged([FromQuery] OrdersPaginationRequest request, CancellationToken ct)
        {
            var (_, branchId) = GetUserAndBranchId();
            var result = await _orderService.GetOrdersPagedAsync(request, branchId, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpGet("summary")]
        public async Task<ActionResult<ServiceResult<DashBoardSummaryDto>>> GetDashboardSummary(CancellationToken ct)
        {
            var (_, branchId) = GetUserAndBranchId();
            var result = await _orderService.GetOrdersSummaryAsync(branchId, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpGet("orders/summary")]
        public async Task<IActionResult> GetOrdersSummary(CancellationToken ct)
        {
            var (_, branchId) = GetUserAndBranchId();
            var result = await _orderService.GetOrdersSummaryAsync(branchId, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpGet("orders/{id}/details")]
        public async Task<ActionResult<ServiceResult<PosOrderDetailsDto>>> GetOrderDetails(int id, CancellationToken ct)
        {
            var (_, branchId) = GetUserAndBranchId();
            var result = await _orderService.GetOrderDetailsAsync(id, branchId, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpPut("orders/{id}")]
        public async Task<ActionResult<ServiceResult<bool>>> UpdateOrder(int id, [FromBody] UpdateOrderDto dto, CancellationToken ct)
        {
            var (_, branchId) = GetUserAndBranchId();
            var result = await _orderService.UpdateOrderAsync(id, branchId, dto, ct);
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
}