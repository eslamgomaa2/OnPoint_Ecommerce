using BuildingBlocks.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.DTOs.Order;
using Onpoint.Store.Application.Services.OrderServ;
using Onpoint.Store.Domin.Enums;
using System.Security.Claims;

[Route("api/orders")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    private int GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var userId))
            throw new UnauthorizedAccessException("Invalid or missing user identifier in token.");
        return userId;
    }

    [HttpGet("{id:int}/items")]
    public async Task<ActionResult<ServiceResult<List<OrderItemDto>>>> GetOrderItems(int id)
    {
        var result = await _orderService.GetOrderItemsAsync(id, GetUserId());
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [Authorize]
    [HttpPost("checkout")]
    public async Task<ActionResult<ServiceResult<OrderDto>>> Checkout([FromBody] CreateOrderDto dto)
    {
        var result = await _orderService.CheckoutAsync(GetUserId(), dto);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [Authorize(Roles = "Customer")]
    [HttpGet("MyOrders")]
    public async Task<ActionResult<ServiceResult<PagedResult<OrderListItemDto>>>> GetMyOrders(
            [FromQuery] GetOrdersQueryDto query, CancellationToken ct)
    {
        var (_, branchId) = GetUserAndBranchId();
        var result = await _orderService.GetDashboardPagedAsync(query, branchId, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpPut("{id}/status")]
    public async Task<ActionResult<ServiceResult<bool>>> UpdateStatus(int id, [FromBody] OrderStatus dto)
    {
        var result = await _orderService.UpdateOrderStatusAsync(id, dto);
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