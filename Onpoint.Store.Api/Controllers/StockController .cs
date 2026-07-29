using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.DTOs.Stock;
using Onpoint.Store.Application.Services.StockServ;
using Onpoint.Store.Domin.Enums;

[Route("api/[controller]")]
[ApiController]
public class StockController : ControllerBase
{
    private readonly IStockService _stockService;
    public StockController(IStockService stockService)
    {
        _stockService = stockService;
    }

    [HttpGet("counts")]
    public async Task<IActionResult> GetCounts(
        [FromQuery] StockStatus status,
        [FromQuery] int? branchId,
        CancellationToken ct)
    {
        var result = status switch
        {
            StockStatus.LowStock => await _stockService.GetLowStockCountAsync(branchId, ct),
            StockStatus.InStock => await _stockService.GetInStockCountAsync(branchId, ct),
            StockStatus.OutOfStock => await _stockService.GetOutOfStockCountAsync(branchId, ct),
            _ => throw new ArgumentOutOfRangeException(nameof(status))
        };
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpGet("products/{productId}")]
    public async Task<IActionResult> GetByProduct(int productId, CancellationToken ct = default)
    {
        var result = await _stockService.GetStockByProductAsync(productId, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [Authorize(Roles = "SuperAdmin" + "," + "BranchManager")]
    [HttpPost("initialize/{Productid}")]
    public async Task<IActionResult> Initialize(int Productid, [FromBody] InitializeStockDto dto, CancellationToken ct = default)
    {
        var result = await _stockService.InitializeStockAsync(Productid, dto, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [Authorize(Roles = "SuperAdmin" + "," + "BranchManager")]
    [HttpPost("adjust")]
    public async Task<IActionResult> Adjust([FromBody] AdjustStockDto dto, CancellationToken ct = default)
    {
        var result = await _stockService.AdjustStockAsync(dto, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [Authorize(Roles = "SuperAdmin" + "," + "BranchManager")]
    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer([FromBody] TransferStockDto dto, CancellationToken ct = default)
    {
        var result = await _stockService.TransferStockAsync(dto, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }
}