using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.DTOs.Order;
using Onpoint.Store.Application.DTOs.PosSales;
using Onpoint.Store.Application.Services.PosSales;
using System.Security.Claims;

[ApiController]
[Route("api/pos-sales")]
[Authorize(Roles = "SuperAdmin" + "," + "Cashier")]
public class PosSalesController : ControllerBase
{
    private readonly IPosSalesService _posSalesService;
    public PosSalesController(IPosSalesService posSalesService)
    {
        _posSalesService = posSalesService;
    }

    [HttpGet]
    public async Task<ActionResult> GetList([FromQuery] PosSalesFilterRequest filter, CancellationToken ct)
    {
        var (userId, branchId) = GetUserAndBranchId();
        var result = await _posSalesService.GetPosSalesPagedAsync(filter, branchId, ct);
        return Ok(result);
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] ExportPosSalesRequestDto filter, CancellationToken ct)
    {
        var (userId, branchId) = GetUserAndBranchId();
        var result = await _posSalesService.ExportToExcelAsync(filter, branchId, ct);
        if (!result.Succeeded || result.Data == null)
            return BadRequest(result);
        return File(result.Data,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"pos-sales-{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }

    [HttpGet("{orderId:int}")]
    public async Task<ActionResult> GetDetails(int orderId, CancellationToken ct)
    {
        var result = await _posSalesService.GetOrderDetailsAsync(orderId, ct);
        return Ok(result);
    }

    [HttpGet("{orderId:int}/receipt")]
    public async Task<ActionResult> GetReceipt(int orderId, CancellationToken ct)
    {
        var result = await _posSalesService.GetReceiptAsync(orderId, ct);
        return Ok(result);
    }

    [HttpGet("{orderId:int}/pdf")]
    public async Task<IActionResult> DownloadPdf(int orderId, CancellationToken ct)
    {
        var result = await _posSalesService.GeneratePdfAsync(orderId, ct);
        if (!result.Succeeded || result.Data == null)
            return BadRequest(result);
        return File(result.Data, "application/pdf", $"invoice-{orderId}.pdf");
    }

    [HttpGet("{orderId:int}/qrcode")]
    public async Task<ActionResult> GenerateQrCode(int orderId, CancellationToken ct)
    {
        var result = await _posSalesService.GenerateAndUploadQrCodeAsync(orderId, ct);
        return Ok(result);
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