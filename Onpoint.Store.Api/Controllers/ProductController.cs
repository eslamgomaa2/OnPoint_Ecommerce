using BuildingBlocks.Results;
using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.DTOs.Product;
using Onpoint.Store.Application.Services.ProductServ;

namespace Onpoint.Store.Api.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> GetFilteredPaged([FromQuery] PaginationRequest request, [FromQuery] int? categoryId = null, CancellationToken ct = default)
        {
            var result = await _productService.GetFilteredPagedAsync(request, categoryId, request.SearchTerm, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpGet("Get/{id}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
        {
            var result = await _productService.GetByIdAsync(id, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Create([FromBody] CreateProductDto dto, CancellationToken ct = default)
        {
            var result = await _productService.CreateAsync(dto, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> Update([FromBody] UpdateProductDto dto, CancellationToken ct = default)
        {
            var result = await _productService.UpdateAsync(dto, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
        {
            var result = await _productService.DeleteAsync(id, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }
    }

}
