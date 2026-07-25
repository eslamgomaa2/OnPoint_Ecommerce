using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.DTOs.Brand;
using Onpoint.Store.Application.Services.Brand;

[ApiController]
[Route("api/brands")]

public class BrandsController : ControllerBase
{
    private readonly IBrandService _brandService;
    public BrandsController(IBrandService brandService)
    {
        _brandService = brandService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _brandService.GetAllAsync();
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpGet("filtered")]
    public async Task<IActionResult> GetFilteredPaged([FromQuery] string? searchTerm, [FromQuery] bool? isActive, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _brandService.GetFilteredPagedAsync(searchTerm, isActive, pageNumber, pageSize);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _brandService.GetByIdAsync(id);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpGet("{id:int}/products")]
    public async Task<IActionResult> GetProductsByBrandId(int id)
    {
        var result = await _brandService.GetProductsByBrandIdAsync(id);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBrandDto dto)
    {
        var result = await _brandService.CreateAsync(dto);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBrandDto dto)
    {
        var result = await _brandService.UpdateAsync(id, dto);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpPatch("{id:int}/toggle-active")]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var result = await _brandService.ToggleActiveAsync(id);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _brandService.DeleteAsync(id);
        return StatusCode((int)result.HttpStatusCode, result);
    }
}