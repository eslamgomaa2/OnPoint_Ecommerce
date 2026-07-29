using Application.Interfaces;
using BuildingBlocks.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.DTOs.Notification;

namespace Onpoint.Store.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _service;

        public NotificationsController(INotificationService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyNotifications([FromQuery] PaginationRequest request)
        {
            var userId = GetCurrentUserId();
            var result = await _service.GetUserNotificationsAsync(userId, request);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpPost]

        public async Task<IActionResult> Create([FromBody] CreateNotificationDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpPut("{id:int}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = GetCurrentUserId();
            var result = await _service.MarkAsReadAsync(id, userId);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst("userId")?.Value
                     ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var id) ? id : 0;
        }
    }
}
