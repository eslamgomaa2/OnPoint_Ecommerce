using BuildingBlocks.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.DTOs.Order;
using Onpoint.Store.Application.Services.OrderServ;
using Onpoint.Store.Domin.Enums;
using System.Security.Claims;

namespace Onpoint.Store.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
        [HttpPost("checkout")]
        public async Task<ActionResult<ServiceResult<OrderDto>>> Checkout([FromBody] CreateOrderDto dto)
        {


            var result = await _orderService.CheckoutAsync(GetUserId(), dto);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpGet("my-orders")]
        public async Task<ActionResult<ServiceResult<IEnumerable<OrderDto>>>> GetMyOrders()
        {
            var result = await _orderService.GetUserOrdersAsync(GetUserId());
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpPut("{id}/status")]
        public async Task<ActionResult<ServiceResult<bool>>> UpdateStatus(int id, [FromBody] OrderStatus dto)
        {
            var result = await _orderService.UpdateOrderStatusAsync(id, dto);
            return StatusCode((int)result.HttpStatusCode, result);
        }






    }
}