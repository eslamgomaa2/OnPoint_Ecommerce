using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.DTOs.Stock;
using Onpoint.Store.Application.Services.StockServ;

namespace Onpoint.Store.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin,BranchManager")]
    public class StockController : ControllerBase
    {
        private readonly IStockService _stockService;

        public StockController(IStockService stockService)
        {
            _stockService = stockService;
        }

        [HttpGet("low-stock/count")]
        public async Task<IActionResult> GetLowStockCount([FromQuery] int? branchId, CancellationToken ct)
        {
            var result = await _stockService.GetLowStockCountAsync(branchId, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }


        [HttpGet("in-stock/count")]
        public async Task<IActionResult> GetInStockCount([FromQuery] int? branchId, CancellationToken ct)
        {
            var result = await _stockService.GetInStockCountAsync(branchId, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }


        [HttpGet("out-of-stock/count")]
        public async Task<IActionResult> GetOutOfStockCount([FromQuery] int? branchId, CancellationToken ct)
        {
            var result = await _stockService.GetOutOfStockCountAsync(branchId, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> Initialize([FromBody] InitializeStockDto dto, CancellationToken ct = default)
        {
            var result = await _stockService.InitializeStockAsync(dto, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Adjust([FromBody] AdjustStockDto dto, CancellationToken ct = default)
        {
            var result = await _stockService.AdjustStockAsync(dto, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Transfer([FromBody] TransferStockDto dto, CancellationToken ct = default)
        {
            var result = await _stockService.TransferStockAsync(dto, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetByProduct(int productId, CancellationToken ct = default)
        {
            var result = await _stockService.GetStockByProductAsync(productId, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }
    }
}