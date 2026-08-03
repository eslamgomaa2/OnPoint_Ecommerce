using BuildingBlocks.Results;
using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.DTOs.Order;
using Onpoint.Store.Application.Services;
using Onpoint.Store.Application.Services.OrderServ;
using System.Security.Claims;

namespace Onpoint.Store.API.Controllers
{

    [Route("api/dashboard")]
    [ApiController]
    // [Authorize(Roles = "SuperAdmin")]
    public class DashboardController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IDashboardService _dashboardService;


        public DashboardController(IOrderService orderService, IDashboardService dashboardService)
        {
            _orderService = orderService;
            _dashboardService = dashboardService;
        }

        [HttpGet("orders")]
        public async Task<ActionResult<ServiceResult<PagedResult<OrderListItemDto>>>> GetDashboardOrders(
            [FromQuery] GetOrdersQueryDto query, CancellationToken ct)
        {
            var (_, branchId) = GetUserAndBranchId();
            var result = await _orderService.GetDashboardPagedAsync(query, branchId, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

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


        [HttpGet("sales-overview")]
        public async Task<IActionResult> GetSalesOverview([FromQuery] int? branchId, CancellationToken ct)
        {
            var result = await _dashboardService.GetSalesOverviewAsync(branchId, ct);
            return StatusCode(result.HttpStatusCode, result);
        }


        [HttpGet("order-status-distribution")]
        public async Task<IActionResult> GetOrderStatusDistribution([FromQuery] int? branchId, CancellationToken ct)
        {
            var result = await _dashboardService.GetOrderStatusDistributionAsync(branchId, ct);
            return StatusCode(result.HttpStatusCode, result);
        }


        [HttpGet("highlights")]
        public async Task<IActionResult> GetHighlights([FromQuery] int? branchId, CancellationToken ct)
        {
            var result = await _dashboardService.GetHighlightsAsync(branchId, ct);
            return StatusCode(result.HttpStatusCode, result);
        }


        [HttpGet("top-customers")]
        public async Task<IActionResult> GetTopCustomers([FromQuery] int count = 10, [FromQuery] int? branchId = null, CancellationToken ct = default)
        {
            var result = await _dashboardService.GetTopCustomersAsync(count, branchId, ct);
            return StatusCode(result.HttpStatusCode, result);
        }


        [HttpGet("top-branches")]
        public async Task<IActionResult> GetTopBranches([FromQuery] int count = 10, CancellationToken ct = default)
        {
            var result = await _dashboardService.GetTopBranchesAsync(count, ct);
            return StatusCode(result.HttpStatusCode, result);
        }


        [HttpGet("latest-orders")]
        public async Task<IActionResult> GetLatestOrders([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 6, [FromQuery] int? branchId = null, CancellationToken ct = default)
        {
            var result = await _dashboardService.GetLatestOrdersAsync(pageNumber, pageSize, branchId, ct);
            return StatusCode(result.HttpStatusCode, result);
        }


        [HttpGet("orders-by-source")]
        public async Task<IActionResult> GetOrdersBySource([FromQuery] int? branchId, CancellationToken ct)
        {
            var result = await _dashboardService.GetOrdersBySourceAsync(branchId, ct);
            return StatusCode(result.HttpStatusCode, result);
        }


        [HttpGet("revenue-by-source")]
        public async Task<IActionResult> GetRevenueBySource([FromQuery] int? branchId, CancellationToken ct)
        {
            var result = await _dashboardService.GetRevenueBySourceAsync(branchId, ct);
            return StatusCode(result.HttpStatusCode, result);
        }


        [HttpGet("pos-sales")]
        public async Task<IActionResult> GetPosSales([FromQuery] int? branchId, CancellationToken ct)
        {
            var result = await _dashboardService.GetPosSalesAsync(branchId, ct);
            return StatusCode(result.HttpStatusCode, result);
        }


        [HttpGet("completed-online-orders")]
        public async Task<IActionResult> GetCompletedOnlineOrders([FromQuery] int? branchId, CancellationToken ct)
        {
            var result = await _dashboardService.GetCompletedOnlineOrdersAsync(branchId, ct);
            return StatusCode(result.HttpStatusCode, result);
        }


        [HttpGet("total-discount-revenue")]
        public async Task<IActionResult> GetTotalDiscountRevenue([FromQuery] int? branchId, CancellationToken ct)
        {
            var result = await _dashboardService.GetTotalDiscountRevenueAsync(branchId, ct);
            return StatusCode(result.HttpStatusCode, result);
        }


        [HttpGet("top-selling-products")]
        public async Task<IActionResult> GetTopSellingProducts([FromQuery] int count = 3, [FromQuery] int? branchId = null, CancellationToken ct = default)
        {
            var result = await _dashboardService.GetTopSellingProductsAsync(count, branchId, ct);
            return StatusCode(result.HttpStatusCode, result);
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