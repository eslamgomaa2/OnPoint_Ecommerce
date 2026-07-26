using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.DTOs.Product;
using Onpoint.Store.Application.Services.ProductVariantServ;

[Route("api/products/{productId}/variants")]
[ApiController]
public class ProductVariantController : ControllerBase
{
    private readonly IProductVariantService _variantService;
    public ProductVariantController(IProductVariantService variantService)
    {
        _variantService = variantService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(int productId, CancellationToken ct = default)
    {
        var result = await _variantService.GetVariantsAsync(productId, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpGet("{variantId}")]
    public async Task<IActionResult> GetById(int productId, int variantId, CancellationToken ct = default)
    {
        var result = await _variantService.GetVariantByIdAsync(productId, variantId, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpPost]
    public async Task<IActionResult> Add(int productId, [FromBody] CreateProductVariantDto dto, CancellationToken ct = default)
    {
        var result = await _variantService.AddVariantAsync(productId, dto, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpPut("{variantId}")]
    public async Task<IActionResult> Update(int productId, int variantId, [FromBody] UpdateProductVariantDto dto, CancellationToken ct = default)
    {
        var result = await _variantService.UpdateVariantAsync(productId, variantId, dto, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpPatch("{variantId}/deactivate")]
    public async Task<IActionResult> Deactivate(int productId, int variantId, CancellationToken ct = default)
    {
        var result = await _variantService.DeactivateVariantAsync(productId, variantId, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpPatch("{variantId}/activate")]
    public async Task<IActionResult> Activate(int productId, int variantId, CancellationToken ct = default)
    {
        var result = await _variantService.ActivateVariantAsync(productId, variantId, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }
}