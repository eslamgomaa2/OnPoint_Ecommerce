using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.DTOs.Product;
using Onpoint.Store.Application.Interfaces;

[ApiController]
[Route("api/products/{productId}/shipping")]
[Authorize(Roles = "SuperAdmin")]
public class ShippingController : ControllerBase
{
    private readonly IShippingService _shippingService;
    public ShippingController(IShippingService shippingService)
    {
        _shippingService = shippingService;
    }

    [HttpGet]
    public async Task<IActionResult> GetByProductId(int productId, CancellationToken ct)
    {
        var result = await _shippingService.GetByProductIdAsync(productId, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(int productId, [FromBody] CreateProductShippingDto dto, CancellationToken ct)
    {
        var result = await _shippingService.CreateAsync(productId, dto, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpPut]
    public async Task<IActionResult> Update(int productId, [FromBody] UpdateProductShippingDto dto, CancellationToken ct)
    {
        var result = await _shippingService.UpdateAsync(productId, dto, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(int productId, CancellationToken ct)
    {
        var result = await _shippingService.DeleteAsync(productId, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }
}