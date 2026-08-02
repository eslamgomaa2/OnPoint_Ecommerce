using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.DTOs.ProductAttribute;
using Onpoint.Store.Application.Services.ProductAttributeServ;

[Route("api/product-attributes")]
[ApiController]
public class ProductAttributeController : ControllerBase
{
    private readonly IProductAttributeService _attributeService;
    public ProductAttributeController(IProductAttributeService attributeService)
    {
        _attributeService = attributeService;
    }


    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
    {
        var result = await _attributeService.GetAllAsync(ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }


    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
    {
        var result = await _attributeService.GetByIdAsync(id, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [Authorize(Roles = "ONPointManager")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductAttributeDto dto, CancellationToken ct = default)
    {
        var result = await _attributeService.CreateAsync(dto, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [Authorize(Roles = "ONPointManager")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductAttributeDto dto, CancellationToken ct = default)
    {
        var result = await _attributeService.UpdateAsync(id, dto, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [Authorize(Roles = "ONPointManager")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        var result = await _attributeService.DeleteAsync(id, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }
}