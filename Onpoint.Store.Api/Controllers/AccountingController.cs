using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.Services.Accounting;

[Route("api/dashboard/accounting")]
[ApiController]
public class AccountingController : ControllerBase
{
    private readonly IAccountingService _accountingService;

    public AccountingController(IAccountingService accountingService)
    {
        _accountingService = accountingService;
    }

    [HttpGet]
    public async Task<IActionResult> GetDashboard([FromQuery] int? branchId, CancellationToken ct)
    {
        var result = await _accountingService.GetAccountingDashboardAsync(branchId, ct);
        return Ok(result);
    }
}