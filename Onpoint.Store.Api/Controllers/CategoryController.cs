using BuildingBlocks.Results;
using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.DTOs.Category;
using Onpoint.Store.Application.Services.CategoryServ;

[Route("api/categories")]
[ApiController]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
    {
        var result = await _categoryService.GetAllAsync(ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] PaginationRequest request, CancellationToken ct = default)
    {
        var result = await _categoryService.GetPagedAsync(request, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
    {
        var result = await _categoryService.GetByIdAsync(id, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto, CancellationToken ct = default)
    {
        var result = await _categoryService.CreateAsync(dto, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto dto, CancellationToken ct = default)
    {
        var result = await _categoryService.UpdateAsync(id, dto, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        var result = await _categoryService.DeleteAsync(id, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }
}