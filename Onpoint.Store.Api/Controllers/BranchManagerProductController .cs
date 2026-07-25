using BuildingBlocks.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.DTOs.Product;
using Onpoint.Store.Application.DTOs.Product.BranchManger;
using Onpoint.Store.Application.Services.BranchManagerProductService;
using Onpoint.Store.Domin.Enums;
using System.Security.Claims;

namespace Onpoint.Store.Api.Controllers
{
    [Route("api/branch-manager/products")]
    [ApiController]
    [Authorize(Roles = "BranchManager")]
    public class BranchManagerProductController : ControllerBase
    {
        private readonly IBranchManagerProductService _branchManagerProductService;

        public BranchManagerProductController(IBranchManagerProductService branchManagerProductService)
        {
            _branchManagerProductService = branchManagerProductService;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardCounts(CancellationToken ct)
        {
            var (_, branchId) = GetUserAndBranchId();
            if (!branchId.HasValue)
                return Unauthorized(new { message = "BranchId not found in token" });

            var result = await _branchManagerProductService.GetDashboardCountsAsync(branchId.Value, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpGet]
        public async Task<IActionResult> GetFilteredPaged(
            [FromQuery] PaginationRequest request,
            [FromQuery] int? categoryId = null,
            [FromQuery] LanguageCode? lang = null,
            CancellationToken ct = default)
        {
            var (_, branchId) = GetUserAndBranchId();
            if (!branchId.HasValue)
                return Unauthorized(new { message = "BranchId not found in token" });

            var result = await _branchManagerProductService.GetFilteredPagedAsync(
                branchId.Value, request, categoryId, request.SearchTerm, lang, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id, [FromQuery] LanguageCode? lang, CancellationToken ct)
        {
            var (_, branchId) = GetUserAndBranchId();
            if (!branchId.HasValue)
                return Unauthorized(new { message = "BranchId not found in token" });

            var result = await _branchManagerProductService.GetByIdAsync(branchId.Value, id, lang, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductByBranchManagerDto dto, CancellationToken ct)
        {
            var (_, branchId) = GetUserAndBranchId();
            if (!branchId.HasValue)
                return Unauthorized(new { message = "BranchId not found in token" });

            var result = await _branchManagerProductService.CreateAsync(branchId.Value, dto, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto, CancellationToken ct)
        {
            var (_, branchId) = GetUserAndBranchId();
            if (!branchId.HasValue)
                return Unauthorized(new { message = "BranchId not found in token" });

            var result = await _branchManagerProductService.UpdateAsync(branchId.Value, id, dto, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var (_, branchId) = GetUserAndBranchId();
            if (!branchId.HasValue)
                return Unauthorized(new { message = "BranchId not found in token" });
            var result = await _branchManagerProductService.DeleteAsync(branchId.Value, id, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        private (int UserId, int? BranchId) GetUserAndBranchId()
        {
            var userClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            var branchClaim = User.FindFirstValue("BranchId");

            if (string.IsNullOrEmpty(userClaim) || !int.TryParse(userClaim, out var userId))
                throw new UnauthorizedAccessException("Invalid or missing user identifier in token.");

            int? branchId = int.TryParse(branchClaim, out var bId) ? bId : null;

            return (userId, branchId);
        }
    }
}