using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.DTOs.Invoice;
using Onpoint.Store.Application.Services.InvoiceServ;
using System.Security.Claims;

namespace Onpoint.Store.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SuperAdmin" + "," + "BranchManager")]
    public class InvoicesController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;

        public InvoicesController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        [HttpGet]
        public async Task<IActionResult> GetInvoices(
            [FromQuery] InvoiceFilter filter,
            CancellationToken ct = default)
        {
            var (_, branchId) = GetUserAndBranchId();

            var result = await _invoiceService.GetInvoicesPagedAsync(branchId, filter, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetInvoiceDetails(
            int id,
            CancellationToken ct = default)
        {
            var (_, branchId) = GetUserAndBranchId();

            var result = await _invoiceService.GetInvoiceDetailsAsync(id, branchId, ct);
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