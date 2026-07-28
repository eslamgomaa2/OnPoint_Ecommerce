using BuildingBlocks.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.DTOs;
using Onpoint.Store.Application.DTOs.Product;
using Onpoint.Store.Application.Services.DiscountServ;
using Onpoint.Store.Application.Services.ProductServ;
using Onpoint.Store.Domin.Enums;
using System.Security.Claims;

[Route("api/admin/products")]
[ApiController]
public class ProductController : ControllerBase
{
    private readonly IProductService productService;
    private readonly IDiscountService _discountService;

    public ProductController(IProductService productService, IDiscountService discountService)
    {
        this.productService = productService;
        _discountService = discountService;
    }
    private int? GetCurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var userId))
            return null;
        return userId;
    }
    [HttpGet]
    public async Task<ActionResult<ServiceResult<PagedResult<ProductListItemDto>>>> GetFiltered(
           [FromQuery] ProductFilterRequestDto filter,
           CancellationToken ct = default)
    {
        var result = await productService.GetFilteredAsync(filter, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }
    [HttpGet("paged")]
    public async Task<IActionResult> GetFilteredPaged(
     [FromQuery] PaginationRequest request,
     [FromQuery] int? categoryId = null,
     [FromQuery] string? searchTerm = null,
     [FromQuery] int? branchId = null,
     [FromQuery] LanguageCode? lang = null,
     CancellationToken ct = default)
    {
        var currentUserId = GetCurrentUserId();
        var result = await productService.GetFilteredPagedAsync(
            request, categoryId, searchTerm, branchId, lang, currentUserId, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, [FromQuery] LanguageCode? lang, CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();
        var result = await productService.GetByIdAsync(id, lang, currentUserId, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboardCounts([FromQuery] int? branchId, CancellationToken ct)
    {
        var result = await productService.GetDashboardCountsAsync(branchId, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpGet("sku/{sku}")]
    public async Task<IActionResult> GetBySku(string sku, CancellationToken ct = default)
    {
        var result = await productService.GetBySkuAsync(sku, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }



    [Authorize(Roles = "SuperAdmin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto, CancellationToken ct)
    {
        var result = await productService.CreateAsync(dto, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto, CancellationToken ct)
    {
        var result = await productService.UpdateAsync(id, dto, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }
    [HttpGet("/api/products/{productId}/discounts")]

    public async Task<IActionResult> GetByProduct(int productId, CancellationToken ct = default)
    {
        var result = await _discountService.GetByProductAsync(productId, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await productService.DeleteAsync(id, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }
}