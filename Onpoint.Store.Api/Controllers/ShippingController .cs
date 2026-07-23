using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.DTOs.Product;
using Onpoint.Store.Application.Interfaces;

namespace Onpoint.Store.API.Controllers
{
    [ApiController]
    [Route("api/shipping")]

    public class ShippingController : ControllerBase
    {
        private readonly IShippingService _shippingService;

        public ShippingController(IShippingService shippingService)
        {
            _shippingService = shippingService;
        }

        [HttpGet("{productId}")]
        public async Task<IActionResult> GetByProductId(int productId, CancellationToken ct)
        {
            var result = await _shippingService.GetByProductIdAsync(productId, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpPost("{productId}")]
        [Authorize(Roles = "Admin,BranchManager")]
        public async Task<IActionResult> Create(int productId, [FromBody] CreateProductShippingDto dto, CancellationToken ct)
        {
            var result = await _shippingService.CreateAsync(productId, dto, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpPut("{productId}")]
        [Authorize(Roles = "Admin,BranchManager")]
        public async Task<IActionResult> Update(int productId, [FromBody] UpdateProductShippingDto dto, CancellationToken ct)
        {
            var result = await _shippingService.UpdateAsync(productId, dto, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpDelete("{productId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int productId, CancellationToken ct)
        {
            var result = await _shippingService.DeleteAsync(productId, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }
    }
}