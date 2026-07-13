using BuildingBlocks.Results;
using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.DTOs.Category;
using Onpoint.Store.Application.Services.CategoryServ;

namespace Onpoint.Store.Api.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetAll(CancellationToken ct = default)
        {
            var result = await _categoryService.GetAllAsync(ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetPaged([FromQuery] PaginationRequest request, CancellationToken ct = default)
        {
            var result = await _categoryService.GetPagedAsync(request, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpGet("Get/{id}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
        {
            var result = await _categoryService.GetByIdAsync(id, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto, CancellationToken ct = default)
        {
            var result = await _categoryService.CreateAsync(dto, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> Update([FromBody] UpdateCategoryDto dto, CancellationToken ct = default)
        {
            var result = await _categoryService.UpdateAsync(dto, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
        {
            var result = await _categoryService.DeleteAsync(id, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }
    }

}
