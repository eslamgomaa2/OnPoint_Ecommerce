using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.DTOs.Product;
using Onpoint.Store.Application.DTOs.ProductVariant;
using Onpoint.Store.Application.Services.ProductVariantServ;

namespace Onpoint.Store.Api.Controllers
{
    [Route("api/products/{productId}/variants")]
    [ApiController]
    [Authorize]
    public class ProductVariantController : ControllerBase
    {
        private readonly IProductVariantService _variantService;

        public ProductVariantController(IProductVariantService variantService)
        {
            _variantService = variantService;
        }


        [HttpGet("/api/variants")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] ProductVariantFilterRequestDto filter, CancellationToken ct)
        {
            var result = await _variantService.GetAllVariantsAsync(filter, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetByProductId(int productId, CancellationToken ct = default)
        {
            var result = await _variantService.GetVariantsByProductIdAsync(productId, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }


        [HttpGet("{variantId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int productId, int variantId, CancellationToken ct = default)
        {
            var result = await _variantService.GetVariantByIdAsync(variantId, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }


        [HttpPost]
        [Authorize(Roles = "SuperAdmin" + "," + "BranchManager")]
        public async Task<IActionResult> Add(int productId, [FromBody] CreateProductVariantDto dto, CancellationToken ct = default)
        {
            var result = await _variantService.AddVariantAsync(productId, dto, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }


        [HttpPut("{variantId}")]
        [Authorize(Roles = "SuperAdmin" + "," + "BranchManager")]
        public async Task<IActionResult> Update(int productId, int variantId, [FromBody] UpdateProductVariantDto dto, CancellationToken ct = default)
        {
            var result = await _variantService.UpdateVariantAsync(productId, variantId, dto, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }


        [HttpDelete("{variantId}")]
        [Authorize(Roles = "SuperAdmin" + "," + "BranchManager")]
        public async Task<IActionResult> Delete(int productId, int variantId, CancellationToken ct = default)
        {
            var result = await _variantService.DeleteVariantAsync(productId, variantId, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }


        [HttpPatch("{variantId}/toggle-status")]
        [Authorize(Roles = "SuperAdmin" + "," + "BranchManager")]
        public async Task<IActionResult> ToggleStatus(int productId, int variantId, CancellationToken ct = default)
        {
            var result = await _variantService.ToggleVariantStatusAsync(productId, variantId, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }
    }
}