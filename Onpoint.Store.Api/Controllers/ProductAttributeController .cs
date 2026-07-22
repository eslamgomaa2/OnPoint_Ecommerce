using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.DTOs.ProductAttribute;
using Onpoint.Store.Application.Services.ProductAttributeServ;

namespace Onpoint.Store.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin,BranchManager")]
    public class ProductAttributeController : ControllerBase
    {
        private readonly IProductAttributeService _attributeService;

        public ProductAttributeController(IProductAttributeService attributeService)
        {
            _attributeService = attributeService;
        }

        [HttpGet]
        [AllowAnonymous]
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

        [HttpPost("[action]")]
        public async Task<IActionResult> Create([FromBody] CreateProductAttributeDto dto, CancellationToken ct = default)
        {
            var result = await _attributeService.CreateAsync(dto, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> Update([FromQuery] int id, [FromBody] UpdateProductAttributeDto dto, CancellationToken ct = default)
        {
            var result = await _attributeService.UpdateAsync(id, dto, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
        {
            var result = await _attributeService.DeleteAsync(id, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }
    }
}