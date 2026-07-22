using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.DTOs.Discount;
using Onpoint.Store.Application.Services.DiscountServ;

namespace Onpoint.Store.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin,BranchManager")]
    public class DiscountController : ControllerBase
    {
        private readonly IDiscountService _discountService;

        public DiscountController(IDiscountService discountService)
        {
            _discountService = discountService;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Add([FromBody] AddDiscountDto dto, CancellationToken ct = default)
        {
            var result = await _discountService.AddDiscountAsync(dto, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpGet("product/{productId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByProduct(int productId, CancellationToken ct = default)
        {
            var result = await _discountService.GetByProductAsync(productId, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpPatch("{discountId}/deactivate")]
        public async Task<IActionResult> Deactivate(int discountId, CancellationToken ct = default)
        {
            var result = await _discountService.DeactivateAsync(discountId, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }
    }
}