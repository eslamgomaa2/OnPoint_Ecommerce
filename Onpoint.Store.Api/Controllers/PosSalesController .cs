using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.DTOs.Order;
using Onpoint.Store.Application.DTOs.PosSales;
using Onpoint.Store.Application.Services.PosSales;
using System.Security.Claims;

[ApiController]
[Route("api/pos-sales")]
//[Authorize(Roles = "SuperAdmin" + "," + "Cashier " + "," + "BranchManager")]
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
    [HttpGet("orders/{orderId}/receipt/pdf")]
    public async Task<IActionResult> DownloadReceiptPdf(int orderId, CancellationToken ct)
    {
        var result = await _posSalesService.GeneratePdfAsync(orderId, ct);

        if (!result.Succeeded || result.Data == null)
            return BadRequest(result.Message ?? "Failed to generate PDF.");

        return File(result.Data, "application/pdf", "Receipt.pdf");
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] ExportPosSalesRequestDto filter, CancellationToken ct)
    {
        var (userId, branchId) = GetUserAndBranchId();
        var result = await _posSalesService.ExportToExcelAsync(filter, branchId, ct);
        if (!result.Succeeded || result.Data == null)
            return BadRequest(result);

        var fileName = $"pos-sales-{DateTime.UtcNow:yyyyMMdd}.xlsx";

        Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{fileName}\"");
        Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
        Response.Headers.Append("Pragma", "no-cache");
        Response.Headers.Append("Expires", "0");

        return File(result.Data,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
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